'use client';

import React, { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Dropdown } from 'primereact/dropdown';
import { InputNumber } from 'primereact/inputnumber';
import { InputTextarea } from 'primereact/inputtextarea';
import { Calendar } from 'primereact/calendar';
import { Toast } from 'primereact/toast';
import { Card } from 'primereact/card';
import { Badge } from 'primereact/badge';
import { ConfirmDialog } from 'primereact/confirmdialog';
import { confirmDialog } from 'primereact/confirmdialog';
import personelZimmetService from '../../../src/services/personelZimmetService';
import zimmetStokService from '../../../src/services/zimmetStokService';
import personelService from '../../../src/services/personelService';
import yetkiService from '../../../src/services/yetkiService';

interface PersonelZimmetData {
    id: number | null;
    personelId: number;
    personelAdSoyad: string;
    departmanAd: string;
    pozisyonAd: string;
    zimmetStokId: number;
    malzemeAdi: string;
    marka: string;
    model: string;
    seriNo: string;
    zimmetMiktar: number;
    zimmetTarihi: Date;
    iadeTarihi: Date | null;
    durum: string;
    zimmetNotu: string;
    iadeNotu: string;
    zimmetVerenId?: number;
    zimmetVerenAdSoyad: string;
    iadeAlanAdSoyad: string;
    aktif: boolean;
    olusturmaTarihi: Date;
    guncellemeTarihi?: Date;
}

interface GroupedZimmetData {
    personelId: number;
    personelAdSoyad: string;
    departmanAd: string;
    pozisyonAd: string;
    personelEmail: string;
    personelTelefon: string;
    toplamZimmet: number;
    aktifZimmet: number;
    iadeEdilmis: number;
    sonZimmetTarihi: Date;
    zimmetDetaylar: ZimmetDetay[];
}

interface ZimmetDetay {
    id: number;
    zimmetStokId: number;
    malzemeAdi: string;
    marka: string;
    model: string;
    seriNo: string;
    zimmetMiktar: number;
    zimmetTarihi: Date;
    iadeTarihi: Date | null;
    durum: string;
    zimmetNotu: string;
    iadeNotu: string;
}

interface PersonelOption {
    label: string;
    value: number;
}

interface StokOption {
    label: string;
    value: number;
    kalanMiktar: number;
    fullName: string;
}

interface ZimmetItem {
    zimmetStokId: number;
    zimmetMiktar: number;
}

interface ZimmetFormData {
    personelId: number;
    zimmetVerenId: number;
    zimmetTarihi: Date;
    zimmetNotu: string;
    zimmetItems: ZimmetItem[];
}

const PersonelZimmet = () => {
    const router = useRouter();
    const [zimmetler, setZimmetler] = useState<PersonelZimmetData[]>([]);
    const [grupluZimmetler, setGrupluZimmetler] = useState<GroupedZimmetData[]>([]);
    const [selectedZimmet, setSelectedZimmet] = useState<PersonelZimmetData | null>(null);
    const [selectedPersonelZimmetleri, setSelectedPersonelZimmetleri] = useState<ZimmetDetay[]>([]);
    const [selectedGrupluZimmet, setSelectedGrupluZimmet] = useState<GroupedZimmetData | null>(null);
    const [zimmetDialog, setZimmetDialog] = useState(false);
    const [iadeDialog, setIadeDialog] = useState(false);
    const [detayDialog, setDetayDialog] = useState(false);
    const [loading, setLoading] = useState(true);
    const [globalFilter, setGlobalFilter] = useState('');
    const [personelOptions, setPersonelOptions] = useState<PersonelOption[]>([]);
    const [stokOptions, setStokOptions] = useState<StokOption[]>([]);
    const toast = useRef<Toast>(null);

    const emptyZimmetForm: ZimmetFormData = {
        personelId: 0,
        zimmetVerenId: 0,
        zimmetTarihi: new Date(),
        zimmetNotu: '',
        zimmetItems: [{ zimmetStokId: 0, zimmetMiktar: 1 }]
    };

    const [zimmetForm, setZimmetForm] = useState<ZimmetFormData>(emptyZimmetForm);
    const [quantityErrors, setQuantityErrors] = useState<{[key: number]: string}>({});

    useEffect(() => {
        loadGrupluZimmetler();
        loadPersonelOptions();
        loadStokOptions();
    }, []);

    const loadZimmetler = async () => {
        try {
            setLoading(true);
            const response = await personelZimmetService.getAll();
            if (response.success) {
                const formattedData = response.data.map((item: any) => ({
                    ...item,
                    zimmetTarihi: new Date(item.zimmetTarihi),
                    iadeTarihi: item.iadeTarihi ? new Date(item.iadeTarihi) : null,
                    olusturmaTarihi: new Date(item.olusturmaTarihi),
                    guncellemeTarihi: item.guncellemeTarihi ? new Date(item.guncellemeTarihi) : undefined
                }));
                setZimmetler(formattedData);
            }
        } catch (error) {
            console.error('Zimmet verileri yüklenemedi:', error);
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Veriler yüklenemedi' });
        } finally {
            setLoading(false);
        }
    };

    const loadGrupluZimmetler = async () => {
        try {
            setLoading(true);
            const response = await personelZimmetService.getGroupedByPersonel();
            if (response.success) {
                const formattedData = response.data.map((item: any) => ({
                    ...item,
                    sonZimmetTarihi: new Date(item.sonZimmetTarihi),
                    zimmetDetaylar: item.zimmetDetaylar.map((detay: any) => ({
                        ...detay,
                        zimmetTarihi: new Date(detay.zimmetTarihi),
                        iadeTarihi: detay.iadeTarihi ? new Date(detay.iadeTarihi) : null
                    }))
                }));
                setGrupluZimmetler(formattedData);
            }
        } catch (error) {
            console.error('Gruplu zimmet verileri yüklenemedi:', error);
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Veriler yüklenemedi' });
        } finally {
            setLoading(false);
        }
    };

    const loadPersonelOptions = async () => {
        try {
            const response = await personelService.getAktif();
            if (response.success) {
                const options = response.data.map((personel: any) => ({
                    label: `${personel.ad} ${personel.soyad} - ${personel.departmanAd}`,
                    value: personel.id
                }));
                setPersonelOptions(options);
            }
        } catch (error) {
            console.error('Personel verileri yüklenemedi:', error);
        }
    };

    const loadStokOptions = async () => {
        try {
            const response = await zimmetStokService.getOnaylananStoklar();
            if (response.success) {
                const options = response.data
                    .filter((stok: any) => stok.kalanMiktar > 0)
                    .map((stok: any) => ({
                        label: `${stok.fullName} (Kalan: ${stok.kalanMiktar} ${stok.birim})`,
                        value: stok.id,
                        kalanMiktar: stok.kalanMiktar,
                        fullName: stok.fullName
                    }));
                setStokOptions(options);
            }
        } catch (error) {
            console.error('Stok verileri yüklenemedi:', error);
        }
    };

    const openNew = () => {
        if (!yetkiService.hasScreenPermission('personel-zimmet', 'create')) {
            toast.current?.show({ severity: 'warn', summary: 'Uyarı', detail: 'Bu işlem için yetkiniz bulunmuyor' });
            return;
        }
        setZimmetForm(emptyZimmetForm);
        setQuantityErrors({});
        setZimmetDialog(true);
    };


    const generateZimmetFormForPersonel = (grupluZimmet: GroupedZimmetData) => {
        // Create a new window for the comprehensive zimmet form
        const newWindow = window.open('', '_blank');
        if (!newWindow) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Yeni pencere açılamadı' });
            return;
        }

        // Get all zimmet items for selected personnel
        const selectedPersonelZimmetler = grupluZimmet.zimmetDetaylar;

        if (selectedPersonelZimmetler.length === 0) {
            toast.current?.show({ severity: 'warn', summary: 'Uyarı', detail: 'Seçilen personele ait zimmet kaydı bulunamadı' });
            newWindow.close();
            return;
        }

        const formHtml = `
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8">
            <title>Zimmet Formu - ${grupluZimmet.personelAdSoyad}</title>
            <style>
                body { font-family: Arial, sans-serif; margin: 20px; font-size: 12px; }
                .header { text-align: center; margin-bottom: 30px; }
                .company-title { font-size: 20px; font-weight: bold; margin-bottom: 10px; }
                .form-title { font-size: 16px; font-weight: bold; margin-bottom: 5px; }
                .form-number { font-size: 14px; margin-bottom: 20px; }
                
                .section { margin-bottom: 25px; border: 1px solid #ccc; }
                .section-header { background-color: #f5f5f5; padding: 8px; font-weight: bold; border-bottom: 1px solid #ccc; }
                .section-content { padding: 15px; }
                
                .field-row { display: flex; margin-bottom: 10px; align-items: center; }
                .field-label { font-weight: bold; min-width: 120px; margin-right: 10px; }
                .field-value { border-bottom: 1px solid #333; padding-bottom: 2px; flex: 1; min-height: 18px; }
                
                .items-table { width: 100%; border-collapse: collapse; margin-top: 10px; }
                .items-table th, .items-table td { border: 1px solid #333; padding: 8px; text-align: left; }
                .items-table th { background-color: #f5f5f5; font-weight: bold; }
                
                .signature-section { display: flex; justify-content: space-between; margin-top: 40px; }
                .signature-box { width: 45%; text-align: center; }
                .signature-area { height: 80px; border: 1px solid #333; margin-bottom: 10px; background-color: #f9f9f9; }
                .signature-line { border-bottom: 1px solid #333; margin-bottom: 5px; padding-bottom: 2px; }
                
                @media print {
                    body { margin: 0; }
                    .no-print { display: none !important; }
                }
            </style>
        </head>
        <body>
            <div class="header">
                <div class="company-title">Icon A.Ş.</div>
                <div class="form-title">KAPSAMLI ZİMMET FORMU</div>
                <div class="form-number">Personel: ${grupluZimmet.personelAdSoyad} | Form Tarihi: ${new Date().toLocaleDateString('tr-TR')}</div>
            </div>

            <div class="section">
                <div class="section-header">PERSONEL BİLGİLERİ</div>
                <div class="section-content">
                    <div class="field-row">
                        <div class="field-label">Ad Soyad:</div>
                        <div class="field-value">${grupluZimmet.personelAdSoyad}</div>
                    </div>
                    <div class="field-row">
                        <div class="field-label">Departman:</div>
                        <div class="field-value">${grupluZimmet.departmanAd}</div>
                    </div>
                    <div class="field-row">
                        <div class="field-label">Pozisyon:</div>
                        <div class="field-value">${grupluZimmet.pozisyonAd}</div>
                    </div>
                </div>
            </div>

            <div class="section">
                <div class="section-header">ZİMMET KALEMLERİ</div>
                <div class="section-content">
                    <table class="items-table">
                        <thead>
                            <tr>
                                <th style="width: 5%;">#</th>
                                <th style="width: 25%;">Malzeme Adı</th>
                                <th style="width: 15%;">Marka/Model</th>
                                <th style="width: 15%;">Seri No</th>
                                <th style="width: 8%;">Miktar</th>
                                <th style="width: 12%;">Zimmet Tarihi</th>
                                <th style="width: 10%;">Durum</th>
                                <th style="width: 10%;">İade Tarihi</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${selectedPersonelZimmetler.map((zimmet, index) => `
                                <tr>
                                    <td>${index + 1}</td>
                                    <td>${zimmet.malzemeAdi}</td>
                                    <td>${zimmet.marka || '-'} ${zimmet.model || ''}</td>
                                    <td>${zimmet.seriNo || '-'}</td>
                                    <td>${zimmet.zimmetMiktar}</td>
                                    <td>${new Date(zimmet.zimmetTarihi).toLocaleDateString('tr-TR')}</td>
                                    <td>${zimmet.durum}</td>
                                    <td>${zimmet.iadeTarihi ? new Date(zimmet.iadeTarihi).toLocaleDateString('tr-TR') : '-'}</td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>
            </div>

            <div class="section">
                <div class="section-header">ÖZET BİLGİLER</div>
                <div class="section-content">
                    <div class="field-row">
                        <div class="field-label">Toplam Zimmet:</div>
                        <div class="field-value">${selectedPersonelZimmetler.length} adet</div>
                    </div>
                    <div class="field-row">
                        <div class="field-label">Aktif Zimmet:</div>
                        <div class="field-value">${selectedPersonelZimmetler.filter(z => z.durum === 'Zimmetli').length} adet</div>
                    </div>
                    <div class="field-row">
                        <div class="field-label">İade Edilmiş:</div>
                        <div class="field-value">${selectedPersonelZimmetler.filter(z => z.durum === 'Iade Edildi').length} adet</div>
                    </div>
                </div>
            </div>

            <div class="signature-section">
                <div class="signature-box">
                    <div style="font-weight: bold; margin-bottom: 10px;">ZİMMET SORUMLUSU</div>
                    <div class="signature-area"></div>
                    <div class="signature-line">___________________</div>
                    <div style="font-size: 10px;">İmza / Tarih</div>
                </div>
                <div class="signature-box">
                    <div style="font-weight: bold; margin-bottom: 10px;">PERSONEL</div>
                    <div class="signature-area"></div>
                    <div class="signature-line">${grupluZimmet.personelAdSoyad}</div>
                    <div style="font-size: 10px;">İmza / Tarih</div>
                </div>
            </div>

            <div style="margin-top: 30px; text-align: center; font-size: 10px; color: #666;">
                Bu form ${new Date().toLocaleDateString('tr-TR')} tarihinde IconIK İK Yönetim Sistemi tarafından otomatik olarak oluşturulmuştur.
            </div>

            <script>
                window.onload = function() {
                    window.print();
                };
            </script>
        </body>
        </html>
        `;

        newWindow.document.write(formHtml);
        newWindow.document.close();
        
        toast.current?.show({ severity: 'success', summary: 'Başarılı', detail: 'Zimmet formu hazırlandı' });
    };

    const hideDialog = () => {
        setZimmetDialog(false);
        setIadeDialog(false);
        setDetayDialog(false);
        setQuantityErrors({});
    };

    const openDetayDialog = (grupluZimmet: GroupedZimmetData) => {
        setSelectedPersonelZimmetleri(grupluZimmet.zimmetDetaylar);
        setSelectedGrupluZimmet(grupluZimmet);
        setDetayDialog(true);
    };

    const openIadeDialogFromDetay = (zimmetDetay: ZimmetDetay) => {
        if (!yetkiService.hasScreenPermission('personel-zimmet', 'update')) {
            toast.current?.show({ severity: 'warn', summary: 'Uyarı', detail: 'Bu işlem için yetkiniz bulunmuyor' });
            return;
        }
        if (zimmetDetay.durum !== 'Zimmetli') {
            toast.current?.show({ severity: 'warn', summary: 'Uyarı', detail: 'Bu zimmet zaten iade edilmiş' });
            return;
        }

        // Convert ZimmetDetay to PersonelZimmetData for compatibility
        const zimmetData: PersonelZimmetData = {
            id: zimmetDetay.id,
            personelId: selectedGrupluZimmet?.personelId || 0,
            personelAdSoyad: selectedGrupluZimmet?.personelAdSoyad || '',
            departmanAd: selectedGrupluZimmet?.departmanAd || '',
            pozisyonAd: selectedGrupluZimmet?.pozisyonAd || '',
            zimmetStokId: zimmetDetay.zimmetStokId,
            malzemeAdi: zimmetDetay.malzemeAdi,
            marka: zimmetDetay.marka,
            model: zimmetDetay.model,
            seriNo: zimmetDetay.seriNo,
            zimmetMiktar: zimmetDetay.zimmetMiktar,
            zimmetTarihi: zimmetDetay.zimmetTarihi,
            iadeTarihi: zimmetDetay.iadeTarihi,
            durum: zimmetDetay.durum,
            zimmetNotu: zimmetDetay.zimmetNotu,
            iadeNotu: zimmetDetay.iadeNotu,
            zimmetVerenAdSoyad: '',
            iadeAlanAdSoyad: '',
            aktif: true,
            olusturmaTarihi: new Date(),
            guncellemeTarihi: undefined
        };

        setSelectedZimmet(zimmetData);
        setDetayDialog(false);
        setIadeDialog(true);
    };

    const addZimmetItem = () => {
        setZimmetForm({
            ...zimmetForm,
            zimmetItems: [...zimmetForm.zimmetItems, { zimmetStokId: 0, zimmetMiktar: 1 }]
        });
    };

    const removeZimmetItem = (index: number) => {
        if (zimmetForm.zimmetItems.length > 1) {
            const newItems = zimmetForm.zimmetItems.filter((_, i) => i !== index);
            setZimmetForm({
                ...zimmetForm,
                zimmetItems: newItems
            });
        }
    };

    const updateZimmetItem = (index: number, field: 'zimmetStokId' | 'zimmetMiktar', value: number) => {
        const newItems = [...zimmetForm.zimmetItems];
        newItems[index][field] = value;

        // Clear previous error for this item
        const newErrors = { ...quantityErrors };
        delete newErrors[index];

        // Validate quantity if it's being updated
        if (field === 'zimmetMiktar') {
            const selectedStok = stokOptions.find(s => s.value === newItems[index].zimmetStokId);
            if (selectedStok && value > selectedStok.kalanMiktar) {
                newErrors[index] = `Maksimum ${selectedStok.kalanMiktar} adet zimmetlenebilir`;
            }
        }

        // Validate quantity if stock is being changed
        if (field === 'zimmetStokId') {
            const selectedStok = stokOptions.find(s => s.value === value);
            if (selectedStok && newItems[index].zimmetMiktar > selectedStok.kalanMiktar) {
                newErrors[index] = `Maksimum ${selectedStok.kalanMiktar} adet zimmetlenebilir`;
            }
        }

        setQuantityErrors(newErrors);
        setZimmetForm({
            ...zimmetForm,
            zimmetItems: newItems
        });
    };

    const saveZimmet = async () => {
        try {
            // Basic validation
            if (!zimmetForm.personelId || !zimmetForm.zimmetVerenId) {
                toast.current?.show({ severity: 'warn', summary: 'Uyarı', detail: 'Lütfen personel ve zimmet veren seçiniz' });
                return;
            }

            // Check for quantity errors
            if (Object.keys(quantityErrors).length > 0) {
                toast.current?.show({
                    severity: 'warn',
                    summary: 'Uyarı',
                    detail: 'Lütfen miktar hatalarını düzeltiniz'
                });
                return;
            }

            // Validate zimmet items
            for (let i = 0; i < zimmetForm.zimmetItems.length; i++) {
                const item = zimmetForm.zimmetItems[i];
                
                if (!item.zimmetStokId || item.zimmetMiktar <= 0) {
                    toast.current?.show({ 
                        severity: 'warn', 
                        summary: 'Uyarı', 
                        detail: `${i + 1}. satırda malzeme seçimi ve miktar girişi zorunludur` 
                    });
                    return;
                }

                const selectedStok = stokOptions.find(s => s.value === item.zimmetStokId);
                if (!selectedStok) {
                    toast.current?.show({ 
                        severity: 'warn', 
                        summary: 'Uyarı', 
                        detail: `${i + 1}. satırda geçersiz malzeme seçimi` 
                    });
                    return;
                }

                if (item.zimmetMiktar > selectedStok.kalanMiktar) {
                    toast.current?.show({ 
                        severity: 'warn', 
                        summary: 'Uyarı', 
                        detail: `${selectedStok.fullName} için yetersiz stok. Maksimum ${selectedStok.kalanMiktar} adet zimmetlenebilir` 
                    });
                    return;
                }
            }

            // Check for duplicate stock selections
            const stockIds = zimmetForm.zimmetItems.map(item => item.zimmetStokId);
            const duplicates = stockIds.filter((id, index) => stockIds.indexOf(id) !== index);
            if (duplicates.length > 0) {
                toast.current?.show({ 
                    severity: 'warn', 
                    summary: 'Uyarı', 
                    detail: 'Aynı malzeme birden fazla kez seçilemez' 
                });
                return;
            }

            const zimmetData = {
                personelId: zimmetForm.personelId,
                zimmetVerenId: zimmetForm.zimmetVerenId,
                zimmetTarihi: zimmetForm.zimmetTarihi,
                zimmetNotu: zimmetForm.zimmetNotu,
                zimmetItems: zimmetForm.zimmetItems
            };

            const response = await personelZimmetService.createBulk(zimmetData);
            if (response.success) {
                toast.current?.show({ 
                    severity: 'success', 
                    summary: 'Başarılı', 
                    detail: response.message || 'Zimmet kayıtları oluşturuldu' 
                });
                hideDialog();
                loadGrupluZimmetler();
                loadStokOptions(); // Reload to update remaining quantities
            } else {
                toast.current?.show({ severity: 'error', summary: 'Hata', detail: response.message });
            }
        } catch (error: any) {
            console.error('Zimmet oluşturma hatası:', error);
            toast.current?.show({ 
                severity: 'error', 
                summary: 'Hata', 
                detail: error?.response?.data?.message || 'Zimmet oluşturulamadı' 
            });
        }
    };

    const openIadeDialog = (zimmetData: PersonelZimmetData) => {
        if (!yetkiService.hasScreenPermission('personel-zimmet', 'update')) {
            toast.current?.show({ severity: 'warn', summary: 'Uyarı', detail: 'Bu işlem için yetkiniz bulunmuyor' });
            return;
        }
        if (zimmetData.durum !== 'Zimmetli') {
            toast.current?.show({ severity: 'warn', summary: 'Uyarı', detail: 'Bu zimmet zaten iade edilmiş' });
            return;
        }
        setSelectedZimmet(zimmetData);
        setIadeDialog(true);
    };

    const iadeEt = async () => {
        try {
            if (!selectedZimmet) return;

            const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
            const iadeData = {
                iadeNotu: selectedZimmet.iadeNotu,
                iadeAlanId: currentUser.personelId
            };

            const response = await personelZimmetService.iadeEt(selectedZimmet.id!, iadeData);
            if (response.success) {
                toast.current?.show({ severity: 'success', summary: 'Başarılı', detail: 'Zimmet iade edildi' });
                hideDialog();
                loadGrupluZimmetler();
                loadStokOptions(); // Reload to update remaining quantities

                // If details dialog was open, refresh its data
                if (detayDialog) {
                    // Find updated personnel data and refresh details
                    const updatedGrupluZimmetler = await personelZimmetService.getGroupedByPersonel();
                    if (updatedGrupluZimmetler.success) {
                        const updatedPersonel = updatedGrupluZimmetler.data.find((p: any) =>
                            p.zimmetDetaylar.some((d: any) => d.id === selectedZimmet.id)
                        );
                        if (updatedPersonel) {
                            setSelectedPersonelZimmetleri(updatedPersonel.zimmetDetaylar.map((detay: any) => ({
                                ...detay,
                                zimmetTarihi: new Date(detay.zimmetTarihi),
                                iadeTarihi: detay.iadeTarihi ? new Date(detay.iadeTarihi) : null
                            })));
                        }
                    }
                }
            } else {
                toast.current?.show({ severity: 'error', summary: 'Hata', detail: response.message });
            }
        } catch (error: any) {
            console.error('İade hatası:', error);
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: error?.response?.data?.message || 'İade işlemi başarısız' });
        }
    };

    const confirmDelete = (zimmetData: PersonelZimmetData) => {
        if (!yetkiService.hasScreenPermission('personel-zimmet', 'delete')) {
            toast.current?.show({ severity: 'warn', summary: 'Uyarı', detail: 'Bu işlem için yetkiniz bulunmuyor' });
            return;
        }
        confirmDialog({
            message: 'Bu zimmet kaydını silmek istediğinizden emin misiniz?',
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deleteZimmet(zimmetData.id!)
        });
    };

    const deleteZimmet = async (id: number) => {
        try {
            const response = await personelZimmetService.delete(id);
            if (response.success) {
                toast.current?.show({ severity: 'success', summary: 'Başarılı', detail: 'Zimmet kaydı silindi' });
                loadGrupluZimmetler();
                loadStokOptions(); // Reload to update remaining quantities
            } else {
                toast.current?.show({ severity: 'error', summary: 'Hata', detail: response.message });
            }
        } catch (error: any) {
            console.error('Silme hatası:', error);
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: error?.response?.data?.message || 'Silme işlemi başarısız' });
        }
    };

    const durumBodyTemplate = (rowData: PersonelZimmetData) => {
        const severity = rowData.durum === 'Zimmetli' ? 'success' : 'info';
        return <Badge value={rowData.durum} severity={severity} />;
    };

    const tarihBodyTemplate = (rowData: PersonelZimmetData, field: string) => {
        const tarih = rowData[field as keyof PersonelZimmetData] as Date;
        return tarih ? tarih.toLocaleDateString('tr-TR') : '-';
    };

    const aktifZimmetBodyTemplate = (rowData: GroupedZimmetData) => {
        return <Badge value={rowData.aktifZimmet} severity="success" />;
    };

    const iadeEdilenBodyTemplate = (rowData: GroupedZimmetData) => {
        return <Badge value={rowData.iadeEdilmis} severity="info" />;
    };

    const sonZimmetTarihBodyTemplate = (rowData: GroupedZimmetData) => {
        return rowData.sonZimmetTarihi.toLocaleDateString('tr-TR');
    };

    const actionBodyTemplate = (rowData: GroupedZimmetData) => {
        return (
            <div className="flex gap-2">
                <Button
                    icon="pi pi-eye"
                    rounded
                    outlined
                    severity="info"
                    onClick={() => openDetayDialog(rowData)}
                    tooltip="Zimmet Detaylarını Görüntüle"
                />
                <Button
                    icon="pi pi-print"
                    rounded
                    outlined
                    severity="secondary"
                    onClick={() => generateZimmetFormForPersonel(rowData)}
                    tooltip="Zimmet Formu Yazdır"
                />
            </div>
        );
    };

    const detayActionBodyTemplate = (rowData: ZimmetDetay) => {
        return (
            <div className="flex gap-1">
                {rowData.durum === 'Zimmetli' && (
                    <Button
                        icon="pi pi-undo"
                        size="small"
                        outlined
                        onClick={() => openIadeDialogFromDetay(rowData)}
                        tooltip="İade Et"
                    />
                )}
            </div>
        );
    };

    const detayDurumBodyTemplate = (rowData: ZimmetDetay) => {
        const severity = rowData.durum === 'Zimmetli' ? 'success' : 'info';
        return <Badge value={rowData.durum} severity={severity} />;
    };

    const detayTarihBodyTemplate = (rowData: ZimmetDetay, field: string) => {
        const tarih = rowData[field as keyof ZimmetDetay] as Date;
        return tarih ? tarih.toLocaleDateString('tr-TR') : '-';
    };

    const header = (
        <div className="flex flex-wrap align-items-center justify-content-between gap-2">
            <span className="text-xl text-900 font-bold">Personel Zimmet İşlemleri</span>
            <div className="flex gap-2">
                <span className="p-input-icon-left">
                    <i className="pi pi-search" />
                    <InputText
                        type="search"
                        placeholder="Ara..."
                        onInput={(e) => setGlobalFilter((e.target as HTMLInputElement).value)}
                    />
                </span>
                <Button label="Yeni Zimmet" icon="pi pi-plus" onClick={openNew} />
            </div>
        </div>
    );

    const zimmetDialogFooter = (
        <div>
            <Button label="İptal" icon="pi pi-times" outlined onClick={hideDialog} />
            <Button label="Kaydet" icon="pi pi-check" onClick={saveZimmet} />
        </div>
    );

    const iadeDialogFooter = (
        <div>
            <Button label="İptal" icon="pi pi-times" outlined onClick={hideDialog} />
            <Button label="İade Et" icon="pi pi-check" onClick={iadeEt} />
        </div>
    );

    return (
        <div className="grid">
            <div className="col-12">
                <Card>
                    <Toast ref={toast} />
                    <ConfirmDialog />
                    
                    <DataTable
                        value={grupluZimmetler}
                        paginator
                        rows={10}
                        dataKey="personelId"
                        loading={loading}
                        globalFilter={globalFilter}
                        header={header}
                        emptyMessage="Zimmet kaydı bulunamadı"
                        size="small"
                    >
                        <Column field="personelAdSoyad" header="Personel" sortable />
                        <Column field="departmanAd" header="Departman" sortable />
                        <Column field="pozisyonAd" header="Pozisyon" sortable />
                        <Column field="toplamZimmet" header="Toplam Zimmet" sortable />
                        <Column
                            field="aktifZimmet"
                            header="Aktif Zimmet"
                            sortable
                            body={aktifZimmetBodyTemplate}
                        />
                        <Column
                            field="iadeEdilmis"
                            header="İade Edilmiş"
                            sortable
                            body={iadeEdilenBodyTemplate}
                        />
                        <Column
                            field="sonZimmetTarihi"
                            header="Son Zimmet Tarihi"
                            sortable
                            body={sonZimmetTarihBodyTemplate}
                        />
                        <Column body={actionBodyTemplate} header="İşlemler" />
                    </DataTable>

                    <Dialog
                        visible={zimmetDialog}
                        style={{ width: '800px' }}
                        header="Yeni Zimmet"
                        modal
                        className="p-fluid"
                        footer={zimmetDialogFooter}
                        onHide={hideDialog}
                    >
                        <div className="grid">
                            <div className="col-6">
                                <div className="field">
                                    <label htmlFor="personel">Personel *</label>
                                    <Dropdown
                                        id="personel"
                                        value={zimmetForm.personelId}
                                        onChange={(e) => setZimmetForm({ ...zimmetForm, personelId: e.value })}
                                        options={personelOptions}
                                        placeholder="Personel seçiniz"
                                        filter
                                        filterBy="label"
                                        showClear
                                    />
                                </div>
                            </div>
                            <div className="col-6">
                                <div className="field">
                                    <label htmlFor="zimmetVeren">Zimmet Veren *</label>
                                    <Dropdown
                                        id="zimmetVeren"
                                        value={zimmetForm.zimmetVerenId}
                                        onChange={(e) => setZimmetForm({ ...zimmetForm, zimmetVerenId: e.value })}
                                        options={personelOptions}
                                        placeholder="Zimmet veren kişiyi seçiniz"
                                        filter
                                        filterBy="label"
                                        showClear
                                    />
                                </div>
                            </div>
                            <div className="col-6">
                                <div className="field">
                                    <label htmlFor="zimmetTarihi">Zimmet Tarihi</label>
                                    <Calendar
                                        id="zimmetTarihi"
                                        value={zimmetForm.zimmetTarihi}
                                        onChange={(e) => setZimmetForm({ ...zimmetForm, zimmetTarihi: e.value || new Date() })}
                                        dateFormat="dd/mm/yy"
                                        showIcon
                                    />
                                </div>
                            </div>
                            <div className="col-6">
                                <div className="field">
                                    <label htmlFor="zimmetNotu">Zimmet Notu</label>
                                    <InputTextarea
                                        id="zimmetNotu"
                                        value={zimmetForm.zimmetNotu}
                                        onChange={(e) => setZimmetForm({ ...zimmetForm, zimmetNotu: e.target.value })}
                                        rows={2}
                                        autoResize
                                    />
                                </div>
                            </div>
                        </div>

                        <div className="mt-3">
                            <div className="flex justify-content-between align-items-center mb-2">
                                <h5>Zimmet Malzemeleri</h5>
                                <Button 
                                    icon="pi pi-plus" 
                                    label="Malzeme Ekle" 
                                    size="small"
                                    onClick={addZimmetItem}
                                />
                            </div>
                            
                            {zimmetForm.zimmetItems.map((item, index) => (
                                <div key={index} className="grid mb-2 p-2 border-1 border-300 border-round">
                                    <div className="col-5">
                                        <div className="field">
                                            <label>Malzeme *</label>
                                            <Dropdown
                                                value={item.zimmetStokId}
                                                onChange={(e) => updateZimmetItem(index, 'zimmetStokId', e.value)}
                                                options={stokOptions}
                                                placeholder="Malzeme seçiniz"
                                                filter
                                                filterBy="label"
                                                showClear
                                            />
                                        </div>
                                    </div>
                                    <div className="col-3">
                                        <div className="field">
                                            <label>Miktar *</label>
                                            <InputNumber
                                                value={item.zimmetMiktar}
                                                onValueChange={(e) => updateZimmetItem(index, 'zimmetMiktar', e.value || 1)}
                                                min={1}
                                                className={quantityErrors[index] ? 'p-invalid' : ''}
                                            />
                                            {quantityErrors[index] && (
                                                <small className="p-error">{quantityErrors[index]}</small>
                                            )}
                                        </div>
                                    </div>
                                    <div className="col-3">
                                        <div className="field">
                                            <label>Kalan Stok</label>
                                            <InputText 
                                                value={String(stokOptions.find(s => s.value === item.zimmetStokId)?.kalanMiktar || '-')}
                                                disabled
                                            />
                                        </div>
                                    </div>
                                    <div className="col-1 flex align-items-end">
                                        <Button 
                                            icon="pi pi-trash" 
                                            className="p-button-danger p-button-outlined"
                                            onClick={() => removeZimmetItem(index)}
                                            disabled={zimmetForm.zimmetItems.length === 1}
                                            tooltip="Kaldır"
                                        />
                                    </div>
                                </div>
                            ))}
                        </div>
                    </Dialog>

                    <Dialog
                        visible={iadeDialog}
                        style={{ width: '450px' }}
                        header="Zimmet İade"
                        modal
                        className="p-fluid"
                        footer={iadeDialogFooter}
                        onHide={hideDialog}
                    >
                        {selectedZimmet && (
                            <>
                                <div className="field">
                                    <label>Personel</label>
                                    <InputText value={selectedZimmet.personelAdSoyad} disabled />
                                </div>

                                <div className="field">
                                    <label>Malzeme</label>
                                    <InputText value={`${selectedZimmet.malzemeAdi} - ${selectedZimmet.marka} ${selectedZimmet.model}`} disabled />
                                </div>

                                <div className="field">
                                    <label>Zimmet Miktarı</label>
                                    <InputNumber value={selectedZimmet.zimmetMiktar} disabled />
                                </div>

                                <div className="field">
                                    <label htmlFor="iadeNotu">İade Notu</label>
                                    <InputTextarea
                                        id="iadeNotu"
                                        value={selectedZimmet.iadeNotu}
                                        onChange={(e) => setSelectedZimmet({ ...selectedZimmet, iadeNotu: e.target.value })}
                                        rows={3}
                                        autoResize
                                    />
                                </div>
                            </>
                        )}
                    </Dialog>

                    <Dialog
                        visible={detayDialog}
                        style={{ width: '1000px' }}
                        header="Personel Zimmet Detayları"
                        modal
                        className="p-fluid"
                        onHide={hideDialog}
                    >
                        <DataTable
                            value={selectedPersonelZimmetleri}
                            dataKey="id"
                            emptyMessage="Zimmet detayı bulunamadı"
                            size="small"
                        >
                            <Column field="malzemeAdi" header="Malzeme" sortable />
                            <Column
                                field="marka"
                                header="Marka/Model"
                                sortable
                                body={(rowData) => `${rowData.marka || ''} ${rowData.model || ''}`.trim() || '-'}
                            />
                            <Column field="seriNo" header="Seri No" sortable />
                            <Column field="zimmetMiktar" header="Miktar" sortable />
                            <Column
                                field="zimmetTarihi"
                                header="Zimmet Tarihi"
                                sortable
                                body={(rowData) => detayTarihBodyTemplate(rowData, 'zimmetTarihi')}
                            />
                            <Column
                                field="iadeTarihi"
                                header="İade Tarihi"
                                sortable
                                body={(rowData) => detayTarihBodyTemplate(rowData, 'iadeTarihi')}
                            />
                            <Column
                                field="durum"
                                header="Durum"
                                body={detayDurumBodyTemplate}
                                sortable
                            />
                            <Column
                                body={detayActionBodyTemplate}
                                header="İşlemler"
                                style={{ width: '100px' }}
                            />
                        </DataTable>
                    </Dialog>

                </Card>
            </div>
        </div>
    );
};

export default PersonelZimmet;