'use client';

import React, { useState, useEffect, useRef, useCallback, useMemo } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { InputTextarea } from 'primereact/inputtextarea';
import { Dropdown } from 'primereact/dropdown';
import { InputNumber } from 'primereact/inputnumber';
import { Calendar } from 'primereact/calendar';
import { FileUpload } from 'primereact/fileupload';
import { Avatar } from 'primereact/avatar';
import { Badge } from 'primereact/badge';
import { confirmDialog } from 'primereact/confirmdialog';
import { Card } from 'primereact/card';
import { Divider } from 'primereact/divider';
import { InputMask } from 'primereact/inputmask';
import { Message } from 'primereact/message';
import { InputSwitch } from 'primereact/inputswitch';
import { TabView, TabPanel } from 'primereact/tabview';
import { Fieldset } from 'primereact/fieldset';
import { RadioButton } from 'primereact/radiobutton';
import { Panel } from 'primereact/panel';
import personelService from '../services/personelService';
import departmanService from '../services/departmanService';
import kademeService from '../services/kademeService';
import pozisyonService from '../services/pozisyonService';
import yetkiService from '../services/yetkiService';
import fileUploadService from '../services/fileUploadService';

const Personeller = () => {
    const [personeller, setPersoneller] = useState([]);
    const [departmanlar, setDepartmanlar] = useState([]);
    const [kademeler, setKademeler] = useState([]);
    const [pozisyonlar, setPozisyonlar] = useState([]);
    const [pozisyonlarFiltered, setPozisyonlarFiltered] = useState([]);
    const [yoneticiler, setYoneticiler] = useState([]);
    const [yoneticilerFiltered, setYoneticilerFiltered] = useState([]);
    // Permission states
    const [permissions, setPermissions] = useState({
        read: false,
        write: false,
        delete: false,
        update: false
    });
    const [selectedDepartmanId, setSelectedDepartmanId] = useState(null);
    const [personelDialog, setPersonelDialog] = useState(false);
    const [personel, setPersonel] = useState({
        id: null,
        tcKimlik: '',
        kullaniciAdi: '',
        ad: '',
        soyad: '',
        email: '',
        telefon: '',
        dogumTarihi: null,
        iseBaslamaTarihi: null,
        cikisTarihi: null,
        pozisyonId: null,
        yoneticiId: null,
        maas: null,
        fotografUrl: '',
        adres: '',
        medeniHal: '',
        cinsiyet: '',
        askerlikDurumu: '',
        egitimDurumu: '',
        kanGrubu: '',
        ehliyetSinifi: '',
        anneAdi: '',
        babaAdi: '',
        dogumYeri: '',
        nufusIlKod: '',
        nufusIlceKod: '',
        acilDurumIletisim: '',
        bankaHesapNo: '',
        ibanNo: '',
        aktif: true
    });
    const [selectedPersoneller, setSelectedPersoneller] = useState(null);
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState('');
    const [filters, setFilters] = useState({
        'tcKimlik': { value: null, matchMode: 'contains' },
        'adSoyad': { value: null, matchMode: 'contains' },
        'pozisyonAd': { value: null, matchMode: 'contains' },
        'departmanAd': { value: null, matchMode: 'contains' },
        'kademeAd': { value: null, matchMode: 'contains' },
        'yoneticiAd': { value: null, matchMode: 'contains' },
        'iseBaslamaTarihi': { value: null, matchMode: 'dateIs' },
        'aktif': { value: null, matchMode: 'equals' }
    });
    const [loading, setLoading] = useState(false);
    const [maasUyarisi, setMaasUyarisi] = useState('');
    const [pozisyonMaasBilgi, setPozisyonMaasBilgi] = useState(null);
    const [uploadedFile, setUploadedFile] = useState(null);
    const [activeTabIndex, setActiveTabIndex] = useState(0);
    const toast = useRef(null);
    const dt = useRef(null);
    const fileUploadRef = useRef(null);

    // Kullanıcı adı validasyon fonksiyonu
    const validateKullaniciAdi = (kullaniciAdi) => {
        if (!kullaniciAdi || kullaniciAdi.trim() === '') {
            return { valid: false, message: 'Kullanıcı adı boş olamaz.' };
        }
        
        // Sadece İngilizce harfler, sayılar, nokta ve alt çizgi
        const regex = /^[a-zA-Z0-9._]+$/;
        if (!regex.test(kullaniciAdi)) {
            return { 
                valid: false, 
                message: 'Kullanıcı adı sadece İngilizce harfler, sayılar, nokta (.) ve alt çizgi (_) içerebilir.' 
            };
        }

        return { valid: true, message: '' };
    };

    useEffect(() => {
        loadData();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('personeller', 'read'),
                write: yetkiService.hasScreenPermission('personeller', 'write'),
                delete: yetkiService.hasScreenPermission('personeller', 'delete'),
                update: yetkiService.hasScreenPermission('personeller', 'update')
            });
        } catch (error) {
            console.error('Permission loading error:', error);
            // If permission loading fails, deny all permissions for safety
            setPermissions({
                read: false,
                write: false,
                delete: false,
                update: false
            });
        }
    };

    const loadData = async () => {
        await Promise.all([
            loadPersoneller(),
            loadDepartmanlar(),
            loadKademeler(),
            loadPozisyonlar(),
            loadYoneticiler()
        ]);
    };

    const loadPersoneller = async () => {
        setLoading(true);
        try {
            const response = await personelService.getAllPersoneller();
            console.log('LoadPersoneller - Backend response:', response);
            if (response.success) {
                console.log('LoadPersoneller - First personel:', response.data[0]);
                setPersoneller(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Personeller yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const loadDepartmanlar = async () => {
        try {
            // Dropdown için sadece aktif departmanları getir
            const response = await departmanService.getAktifDepartmanlar();
            if (response.success) {
                setDepartmanlar(response.data);
            }
        } catch (error) {
            console.error('Departmanlar yüklenirken hata:', error);
        }
    };

    const loadKademeler = async () => {
        try {
            const response = await kademeService.getAllKademeler();
            if (response.success) {
                setKademeler(response.data);
            }
        } catch (error) {
            console.error('Kademeler yüklenirken hata:', error);
        }
    };

    const loadPozisyonlar = async () => {
        try {
            // Dropdown için sadece aktif pozisyonları getir
            const response = await pozisyonService.getAktifPozisyonlar();
            if (response.success) {
                setPozisyonlar(response.data);
            }
        } catch (error) {
            console.error('Pozisyonlar yüklenirken hata:', error);
        }
    };

    const loadYoneticiler = async () => {
        try {
            const response = await personelService.getYoneticiler();
            if (response.success) {
                setYoneticiler(response.data);
            }
        } catch (error) {
            console.error('Yöneticiler yüklenirken hata:', error);
        }
    };

    const onDepartmanChange = (e) => {
        const departmanId = e.value;
        setSelectedDepartmanId(departmanId);
        setPersonel({ ...personel, pozisyonId: null, yoneticiId: null });
        
        if (departmanId) {
            const filtered = pozisyonlar.filter(p => p.departmanId === departmanId);
            setPozisyonlarFiltered(filtered);
        } else {
            setPozisyonlarFiltered([]);
        }
        
        // Pozisyon ve yönetici seçimlerini resetle
        setYoneticilerFiltered([]);
        setMaasUyarisi('');
        setPozisyonMaasBilgi(null);
    };

    const onPozisyonChange = useCallback(async (e) => {
        const pozisyonId = e.value;
        setPersonel(prevPersonel => ({ 
            ...prevPersonel, 
            pozisyonId, 
            yoneticiId: null 
        }));
        setMaasUyarisi('');

        if (pozisyonId) {
            try {
                const response = await personelService.getMaasKontroluBilgi(pozisyonId);
                if (response.success) {
                    setPozisyonMaasBilgi(response.data);
                }
            } catch (error) {
                console.error('Pozisyon maaş bilgisi alınırken hata:', error);
            }

            // Seçilen pozisyonun kademe seviyesini bul
            const secilenPozisyon = pozisyonlarFiltered.find(p => p.id === pozisyonId);
            
            if (secilenPozisyon && pozisyonlarFiltered.length > 0) {
                const departmanId = secilenPozisyon.departmanId;
                const pozisyonKademeSeviye = secilenPozisyon.kademeSeviye;
                
                // Yöneticileri filtrele: 
                // 1. Aynı departmanda ve daha üst kademede (daha düşük seviye) olanlar
                // 2. Genel Müdür (seviye 1) her koşulda dahil
                const filteredYoneticiler = yoneticiler.filter(y => 
                    (y.departmanId === departmanId && y.kademeSeviye < pozisyonKademeSeviye) ||
                    y.kademeSeviye === 1 // Genel Müdür her zaman dahil
                );
                setYoneticilerFiltered(filteredYoneticiler);
            } else {
                setYoneticilerFiltered([]);
            }
        } else {
            setPozisyonMaasBilgi(null);
            setYoneticilerFiltered([]);
        }
    }, [pozisyonlarFiltered, yoneticiler]);

    const onMaasChange = useCallback((e) => {
        const maas = e.value;
        setPersonel(prevPersonel => ({ 
            ...prevPersonel, 
            maas 
        }));
        
        if (maas && pozisyonMaasBilgi) {
            let uyari = '';
            if (pozisyonMaasBilgi.minMaas && maas < pozisyonMaasBilgi.minMaas) {
                uyari = `Uyarı: Girilen maaş (₺${maas.toLocaleString()}), pozisyonun minimum maaşından (₺${pozisyonMaasBilgi.minMaas.toLocaleString()}) düşüktür.`;
            } else if (pozisyonMaasBilgi.maxMaas && maas > pozisyonMaasBilgi.maxMaas) {
                uyari = `Uyarı: Girilen maaş (₺${maas.toLocaleString()}), pozisyonun maksimum maaşından (₺${pozisyonMaasBilgi.maxMaas.toLocaleString()}) yüksektir.`;
            }
            setMaasUyarisi(uyari);
        } else {
            setMaasUyarisi('');
        }
    }, [pozisyonMaasBilgi]);

    const openNew = () => {
        setPersonel({
            id: null,
            tcKimlik: '',
            kullaniciAdi: '',
            ad: '',
            soyad: '',
            email: '',
            telefon: '',
            dogumTarihi: null,
            iseBaslamaTarihi: null,
            cikisTarihi: null,
            pozisyonId: null,
            yoneticiId: null,
            maas: null,
            fotografUrl: '',
            adres: '',
            medeniHal: '',
            cinsiyet: '',
            askerlikDurumu: '',
            egitimDurumu: '',
            kanGrubu: '',
            ehliyetSinifi: '',
            anneAdi: '',
            babaAdi: '',
            dogumYeri: '',
            nufusIlKod: '',
            nufusIlceKod: '',
            acilDurumIletisim: '',
            bankaHesapNo: '',
            ibanNo: '',
            aktif: true
        });
        setSubmitted(false);
        setMaasUyarisi('');
        setPozisyonMaasBilgi(null);
        setPozisyonlarFiltered([]);
        setYoneticilerFiltered([]);
        setSelectedDepartmanId(null);
        setUploadedFile(null);
        setActiveTabIndex(0);
        setPersonelDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setPersonelDialog(false);
        setMaasUyarisi('');
        setPozisyonMaasBilgi(null);
        setUploadedFile(null);
        setActiveTabIndex(0);
    };

    const savePersonel = async () => {
        setSubmitted(true);

        // Tüm zorunlu alanları kontrol et ve hangi sekmede hata olduğunu belirle
        const validateRequiredFields = () => {
            const errors = [];
            let firstErrorTabIndex = -1;

            // Genel Bilgiler sekmesi zorunlu alanları (Tab index: 0)
            if (!personel.tcKimlik || personel.tcKimlik.length !== 11) {
                errors.push('TC Kimlik No (11 haneli)');
                if (firstErrorTabIndex === -1) firstErrorTabIndex = 0;
            }
            if (!personel.ad || !personel.ad.trim()) {
                errors.push('Ad');
                if (firstErrorTabIndex === -1) firstErrorTabIndex = 0;
            }
            if (!personel.soyad || !personel.soyad.trim()) {
                errors.push('Soyad');
                if (firstErrorTabIndex === -1) firstErrorTabIndex = 0;
            }

            // Pozisyon Bilgileri sekmesi zorunlu alanları (Tab index: 1)
            if (!personel.pozisyonId) {
                errors.push('Pozisyon');
                if (firstErrorTabIndex === -1) firstErrorTabIndex = 1;
            }
            if (!personel.iseBaslamaTarihi) {
                errors.push('İşe Başlama Tarihi');
                if (firstErrorTabIndex === -1) firstErrorTabIndex = 1;
            }

            return { errors, firstErrorTabIndex };
        };

        // Kullanıcı adı validasyonu (sadece güncelleme için)
        if (personel.id && personel.kullaniciAdi) {
            const kullaniciAdiValidation = validateKullaniciAdi(personel.kullaniciAdi);
            if (!kullaniciAdiValidation.valid) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: kullaniciAdiValidation.message,
                    life: 3000
                });
                return;
            }
        }

        // Zorunlu alan kontrolü
        const validation = validateRequiredFields();
        if (validation.errors.length > 0) {
            // Hataya sahip ilk sekmeye geç
            if (validation.firstErrorTabIndex !== -1) {
                setActiveTabIndex(validation.firstErrorTabIndex);
            }
            
            toast.current.show({
                severity: 'error',
                summary: 'Eksik Bilgiler',
                detail: `Tüm zorunlu alanları doldurunuz! Eksik alanlar: ${validation.errors.join(', ')}`,
                life: 5000
            });
            return;
        }

        if (personel.tcKimlik.length === 11 && personel.ad.trim() && personel.soyad.trim() && 
            personel.iseBaslamaTarihi && personel.pozisyonId) {
            
            try {
                // Fotoğraf yüklendi ise dosya adını güncelle
                if (uploadedFile) {
                    personel.fotografUrl = uploadedFile.fileName;
                }

                let response;
                if (personel.id) {
                    response = await personelService.updatePersonel(personel.id, personel);
                } else {
                    response = await personelService.createPersonel(personel);
                }

                if (response.success) {
                    // Eğer fotoğraf güncellendiyse avatar cache'ini temizle
                    if (uploadedFile) {
                        fileUploadService.refreshAvatarCache();
                    }
                    
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: response.message,
                        life: 3000
                    });

                    // Yeni personel oluşturulduğunda kullanıcı bilgilerini göster
                    if (!personel.id && response.data.kullaniciAdi) {
                        toast.current.show({
                            severity: 'info',
                            summary: 'Kullanıcı Hesabı Oluşturuldu',
                            detail: `Kullanıcı Adı: ${response.data.kullaniciAdi}, Şifre: ${response.data.defaultSifre}`,
                            life: 8000
                        });
                    }

                    // Maaş uyarısı varsa göster
                    if (response.maasUyarisi) {
                        toast.current.show({
                            severity: 'warn',
                            summary: 'Maaş Uyarısı',
                            detail: response.maasUyarisi,
                            life: 5000
                        });
                    }

                    loadPersoneller();
                    loadYoneticiler(); // Yönetici listesini güncelle
                    setPersonelDialog(false);
                }
            } catch (error) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: error.message,
                    life: 3000
                });
            }
        }
    };

    const editPersonel = async (personel) => {
        console.log('EditPersonel - Received personel data:', personel);
        console.log('EditPersonel - KullaniciAdi value:', personel.kullaniciAdi);
        
        // Fetch complete personel data including kullaniciAdi
        try {
            const response = await personelService.getPersonelById(personel.id);
            if (response.success) {
                console.log('EditPersonel - Fresh data from API:', response.data);
                console.log('EditPersonel - Fresh KullaniciAdi:', response.data.kullaniciAdi);
                personel = response.data; // Use fresh data from API
            }
        } catch (error) {
            console.error('Error fetching fresh personel data:', error);
        }
        
        // Departman seçildikten sonra pozisyonları filtrele
        if (personel.pozisyonId) {
            const pozisyon = pozisyonlar.find(p => p.id === personel.pozisyonId);
            if (pozisyon) {
                setSelectedDepartmanId(pozisyon.departmanId);
                const filtered = pozisyonlar.filter(p => p.departmanId === pozisyon.departmanId);
                setPozisyonlarFiltered(filtered);
                
                // Yönetici filtreleme - seçilen pozisyondan üst kademedekiler + Genel Müdür
                const pozisyonKademeSeviye = pozisyon.kademeSeviye;
                const filteredYoneticiler = yoneticiler.filter(y => 
                    (y.departmanId === pozisyon.departmanId && y.kademeSeviye < pozisyonKademeSeviye) ||
                    y.kademeSeviye === 1 // Genel Müdür her zaman dahil
                );
                setYoneticilerFiltered(filteredYoneticiler);
                
                // Maaş bilgisini yükle
                try {
                    const response = await personelService.getMaasKontroluBilgi(personel.pozisyonId);
                    if (response.success) {
                        setPozisyonMaasBilgi(response.data);
                    }
                } catch (error) {
                    console.error('Pozisyon maaş bilgisi alınırken hata:', error);
                }
            }
        }

        setPersonel({
            ...personel,
            // Convert null values to empty strings for text inputs
            email: personel.email || '',
            telefon: personel.telefon || '',
            fotografUrl: personel.fotografUrl || '',
            adres: personel.adres || '',
            kullaniciAdi: personel.kullaniciAdi || '',
            medeniHal: personel.medeniHal || '',
            cinsiyet: personel.cinsiyet || '',
            askerlikDurumu: personel.askerlikDurumu || '',
            egitimDurumu: personel.egitimDurumu || '',
            kanGrubu: personel.kanGrubu || '',
            ehliyetSinifi: personel.ehliyetSinifi || '',
            anneAdi: personel.anneAdi || '',
            babaAdi: personel.babaAdi || '',
            dogumYeri: personel.dogumYeri || '',
            nufusIlKod: personel.nufusIlKod || '',
            nufusIlceKod: personel.nufusIlceKod || '',
            acilDurumIletisim: personel.acilDurumIletisim || '',
            bankaHesapNo: personel.bankaHesapNo || '',
            ibanNo: personel.ibanNo || '',
            // Handle dates
            dogumTarihi: personel.dogumTarihi ? new Date(personel.dogumTarihi) : null,
            iseBaslamaTarihi: personel.iseBaslamaTarihi ? new Date(personel.iseBaslamaTarihi) : null,
            cikisTarihi: personel.cikisTarihi ? new Date(personel.cikisTarihi) : null
        });
        setPersonelDialog(true);
    };

    const confirmDeletePersonel = (personel) => {
        confirmDialog({
            message: `${personel.ad} ${personel.soyad} adlı personeli silmek istediğinizden emin misiniz?`,
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deletePersonel(personel.id)
        });
    };

    const deletePersonel = async (id) => {
        try {
            const response = await personelService.deletePersonel(id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadPersoneller();
                loadYoneticiler();
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message,
                life: 3000
            });
        }
    };

    const onFileUpload = async (event) => {
        const file = event.files[0];
        
        try {
            const response = await fileUploadService.uploadAvatar(file);
            if (response.success) {
                // Avatar cache'ini temizle
                fileUploadService.refreshAvatarCache();
                setUploadedFile(response.data);
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Fotoğraf başarıyla yüklendi',
                    life: 3000
                });
                fileUploadRef.current.clear();
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message,
                life: 3000
            });
        }
    };

    const removeUploadedFile = () => {
        setUploadedFile(null);
        setPersonel({ ...personel, fotografUrl: '' });
    };

    const exportCSV = () => {
        dt.current.exportCSV();
    };

    const onInputChange = useCallback((e, name) => {
        const val = (e.target && e.target.value) || '';
        setPersonel(prevPersonel => ({
            ...prevPersonel,
            [name]: val
        }));
    }, []);

    const onDropdownChange = useCallback((e, name) => {
        const val = e.value;
        setPersonel(prevPersonel => ({
            ...prevPersonel,
            [name]: val
        }));
    }, []);

    const onDateChange = useCallback((e, name) => {
        const val = e.value;
        setPersonel(prevPersonel => ({
            ...prevPersonel,
            [name]: val
        }));
    }, []);

    // Memoized dropdown options for better performance
    const medeniHalOptions = useMemo(() => [
        { label: 'Bekar', value: 'Bekar' },
        { label: 'Evli', value: 'Evli' },
        { label: 'Boşanmış', value: 'Bosanmis' },
        { label: 'Dul', value: 'Dul' }
    ], []);

    const cinsiyetOptions = useMemo(() => [
        { label: 'Erkek', value: 'Erkek' },
        { label: 'Kadın', value: 'Kadin' }
    ], []);

    const askerlikDurumuOptions = useMemo(() => [
        { label: 'Yapıldı', value: 'Yapildi' },
        { label: 'Tecilli', value: 'Tecilli' },
        { label: 'Muaf', value: 'Muaf' },
        { label: 'Yapmadı', value: 'Yapmadi' }
    ], []);

    const egitimDurumuOptions = useMemo(() => [
        { label: 'İlkokul', value: 'Ilkokul' },
        { label: 'Ortaokul', value: 'Ortaokul' },
        { label: 'Lise', value: 'Lise' },
        { label: 'Ön Lisans', value: 'OnLisans' },
        { label: 'Lisans', value: 'Lisans' },
        { label: 'Yüksek Lisans', value: 'YuksekLisans' },
        { label: 'Doktora', value: 'Doktora' }
    ], []);

    const kanGrubuOptions = useMemo(() => [
        { label: 'A+', value: 'A+' },
        { label: 'A-', value: 'A-' },
        { label: 'B+', value: 'B+' },
        { label: 'B-', value: 'B-' },
        { label: 'AB+', value: 'AB+' },
        { label: 'AB-', value: 'AB-' },
        { label: 'O+', value: 'O+' },
        { label: 'O-', value: 'O-' }
    ], []);

    const leftToolbarTemplate = useMemo(() => {
        return (
            <React.Fragment>
                {permissions.write && (
                    <Button
                        label="Yeni Personel"
                        icon="pi pi-plus"
                        className="p-button-success p-mr-2"
                        onClick={openNew}
                    />
                )}
            </React.Fragment>
        );
    }, [permissions.write, openNew]);

    const rightToolbarTemplate = useMemo(() => {
        return (
            <React.Fragment>
                <Button
                    label="Dışa Aktar"
                    icon="pi pi-upload"
                    className="p-button-help"
                    onClick={exportCSV}
                />
            </React.Fragment>
        );
    }, [exportCSV]);

    const actionBodyTemplate = (rowData) => {
        return (
            <React.Fragment>
                {permissions.update && (
                    <Button
                        icon="pi pi-pencil"
                        className="p-button-rounded p-button-success p-mr-2"
                        onClick={() => editPersonel(rowData)}
                        tooltip="Düzenle"
                    />
                )}
                {permissions.delete && (
                    <Button
                        icon="pi pi-trash"
                        className="p-button-rounded p-button-warning"
                        onClick={() => confirmDeletePersonel(rowData)}
                        tooltip="Sil"
                    />
                )}
                {!permissions.update && !permissions.delete && (
                    <span className="text-500">Yetki yok</span>
                )}
            </React.Fragment>
        );
    };

    const avatarBodyTemplate = (rowData) => {
        if (rowData.fotografUrl) {
            return (
                <Avatar
                    image={fileUploadService.getAvatarUrl(rowData.fotografUrl)}
                    size="large"
                    shape="circle"
                />
            );
        } else {
            return (
                <Avatar
                    label={rowData.ad.charAt(0) + rowData.soyad.charAt(0)}
                    size="large"
                    shape="circle"
                    style={{ backgroundColor: '#2196F3', color: '#ffffff' }}
                />
            );
        }
    };

    const adSoyadWithAvatarTemplate = (rowData) => {
        const avatar = rowData.fotografUrl ? (
            <Avatar
                image={fileUploadService.getAvatarUrl(rowData.fotografUrl)}
                size="normal"
                shape="circle"
            />
        ) : (
            <Avatar
                label={rowData.ad.charAt(0) + rowData.soyad.charAt(0)}
                size="normal"
                shape="circle"
                style={{ backgroundColor: '#2196F3', color: '#ffffff' }}
            />
        );

        return (
            <div className="flex align-items-center gap-2">
                {avatar}
                <span>{rowData.adSoyad}</span>
            </div>
        );
    };

    const maasBodyTemplate = (rowData) => {
        if (rowData.maas) {
            return `₺${rowData.maas.toLocaleString()}`;
        }
        return '-';
    };

    const kademeBodyTemplate = (rowData) => {
        let severity = 'info';
        
        if (rowData.kademeSeviye === 1) {
            severity = 'danger'; // Genel Müdür
        } else if (rowData.kademeSeviye <= 3) {
            severity = 'warning'; // Direktör, Grup Müdürü
        } else if (rowData.kademeSeviye <= 5) {
            severity = 'success'; // Müdür, Yönetici
        }

        return (
            <Badge
                value={rowData.kademeAd}
                severity={severity}
            />
        );
    };

    const izinHakkiBodyTemplate = (rowData) => {
        return `${rowData.kalanIzinHakki || 0} gün`;
    };

    const header = (
        <div className="flex justify-content-between align-items-center">
            <h5 className="m-0">Personel Yönetimi</h5>
            <span className="p-input-icon-left" style={{ width: '300px' }}>
                <i className="pi pi-search" />
                <InputText
                    type="search"
                    onInput={(e) => setGlobalFilter(e.target.value)}
                    placeholder="Genel arama..."
                    className="w-full"
                    size="small"
                />
            </span>
        </div>
    );

    const personelDialogFooter = (
        <React.Fragment>
            <Button
                label="İptal"
                icon="pi pi-times"
                className="p-button-text"
                onClick={hideDialog}
            />
            <Button
                label="Kaydet"
                icon="pi pi-check"
                className="p-button-text"
                onClick={savePersonel}
            />
        </React.Fragment>
    );

    const createdAtBodyTemplate = (rowData) => {
        return new Date(rowData.createdAt).toLocaleDateString('tr-TR');
    };

    const statusFilterTemplate = (options) => {
        return (
            <Dropdown
                value={options.value}
                options={[
                    { label: 'Aktif', value: true },
                    { label: 'Pasif', value: false }
                ]}
                onChange={(e) => options.filterApplyCallback(e.value)}
                optionLabel="label"
                placeholder="Durum"
                className="p-column-filter"
                showClear
                style={{
                    width: '85%',
                    fontSize: '0.8rem'
                }}
                dropdownStyle={{
                    height: '2rem',
                    fontSize: '0.8rem'
                }}
                panelStyle={{
                    fontSize: '0.8rem'
                }}
            />
        );
    };

    const dateFilterTemplate = (options) => {
        return (
            <Calendar
                value={options.value}
                onChange={(e) => options.filterApplyCallback(e.value)}
                dateFormat="dd/mm/yy"
                placeholder="Tarih"
                mask="99/99/9999"
                showIcon
                firstDayOfWeek={1}
                className="p-column-filter"
                style={{
                    width: '85%',
                    fontSize: '0.8rem'
                }}
                inputStyle={{
                    height: '2rem',
                    fontSize: '0.8rem',
                    padding: '0.3rem 0.5rem'
                }}
            />
        );
    };

    const statusBodyTemplate = (rowData) => {
        return (
            <Badge
                value={rowData.aktif ? 'Aktif' : 'Pasif'}
                severity={rowData.aktif ? 'success' : 'warning'}
            />
        );
    };

    const iseBaslamaBodyTemplate = (rowData) => {
        return new Date(rowData.iseBaslamaTarihi).toLocaleDateString('tr-TR');
    };

    // Yönetici dropdown'ı için template - listede pozisyon da gösterir
    const yoneticiItemTemplate = (option) => {
        if (!option) return null;
        return (
            <div>
                <span style={{ fontWeight: 'normal' }}>{option.adSoyad}</span>
                <span style={{ color: '#6c757d', fontSize: '0.875rem', marginLeft: '8px' }}>
                    - {option.pozisyonAd}
                </span>
            </div>
        );
    };

    return (
        <div className="datatable-crud-demo">
            <Toast ref={toast} />

            <Card>
                <Toolbar
                    className="p-mb-4"
                    left={leftToolbarTemplate}
                    right={rightToolbarTemplate}
                ></Toolbar>

                <DataTable
                    ref={dt}
                    value={personeller}
                    selection={selectedPersoneller}
                    onSelectionChange={(e) => setSelectedPersoneller(e.value)}
                    dataKey="id"
                    paginator
                    rows={10}
                    rowsPerPageOptions={[5, 10, 25]}
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                    currentPageReportTemplate="{first} - {last} arası, toplam {totalRecords} kayıt"
                    globalFilter={globalFilter}
                    filters={filters}
                    onFilter={(e) => setFilters(e.filters)}
                    filterDisplay="row"
                    header={header}
                    responsiveLayout="scroll"
                    loading={loading}
                    emptyMessage="Personel bulunamadı."
                    sortField="id"
                    sortOrder={-1}
                    size="small"
                    rowClassName={() => 'p-datatable-row-sm'}
                >
                    <Column
                        field="id"
                        header="Sicil Numarası"
                        sortable
                        style={{ minWidth: '8rem', width: '8rem' }}
                    ></Column>
                    <Column
                        field="kullaniciAdi"
                        header="Kullanıcı Adı"
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="adSoyad"
                        header={(
                            <div className="flex align-items-center gap-1">
                                <span>Ad Soyad</span>
                                <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                            </div>
                        )}
                        body={adSoyadWithAvatarTemplate}
                        sortable
                        filter
                        filterPlaceholder="Ara..."
                        filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                        filterStyle={{
                            fontSize: '0.8rem',
                            padding: '0.4rem 0.5rem',
                            height: '2rem',
                            width: '85%',
                            lineHeight: '1.2',
                            minHeight: '2rem'
                        }}
                        style={{ minWidth: '14rem' }}
                    ></Column>
                    <Column
                        field="tcKimlik"
                        header={(
                            <div className="flex align-items-center gap-1">
                                <span>TC Kimlik</span>
                                <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                            </div>
                        )}
                        sortable
                        filter
                        filterPlaceholder="Ara..."
                        filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                        filterStyle={{
                            fontSize: '0.8rem',
                            padding: '0.4rem 0.5rem',
                            height: '2rem',
                            width: '85%',
                            lineHeight: '1.2',
                            minHeight: '2rem'
                        }}
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="pozisyonAd"
                        header={(
                            <div className="flex align-items-center gap-1">
                                <span>Pozisyon</span>
                                <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                            </div>
                        )}
                        sortable
                        filter
                        filterPlaceholder="Ara..."
                        filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                        filterStyle={{
                            fontSize: '0.8rem',
                            padding: '0.4rem 0.5rem',
                            height: '2rem',
                            width: '85%',
                            lineHeight: '1.2',
                            minHeight: '2rem'
                        }}
                        style={{ minWidth: '12rem' }}
                    ></Column>
                    <Column
                        field="departmanAd"
                        header={(
                            <div className="flex align-items-center gap-1">
                                <span>Departman</span>
                                <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                            </div>
                        )}
                        sortable
                        filter
                        filterPlaceholder="Ara..."
                        filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                        filterStyle={{
                            fontSize: '0.8rem',
                            padding: '0.4rem 0.5rem',
                            height: '2rem',
                            width: '85%',
                            lineHeight: '1.2',
                            minHeight: '2rem'
                        }}
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="kademeAd"
                        header={(
                            <div className="flex align-items-center gap-1">
                                <span>Kademe</span>
                                <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                            </div>
                        )}
                        body={kademeBodyTemplate}
                        sortable
                        filter
                        filterPlaceholder="Ara..."
                        filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                        filterStyle={{
                            fontSize: '0.8rem',
                            padding: '0.4rem 0.5rem',
                            height: '2rem',
                            width: '85%',
                            lineHeight: '1.2',
                            minHeight: '2rem'
                        }}
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="yoneticiAd"
                        header={(
                            <div className="flex align-items-center gap-1">
                                <span>Yönetici</span>
                                <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                            </div>
                        )}
                        sortable
                        filter
                        filterPlaceholder="Ara..."
                        filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                        filterStyle={{
                            fontSize: '0.8rem',
                            padding: '0.4rem 0.5rem',
                            height: '2rem',
                            width: '85%',
                            lineHeight: '1.2',
                            minHeight: '2rem'
                        }}
                        style={{ minWidth: '12rem' }}
                    ></Column>
                    <Column
                        field="maas"
                        header="Maaş"
                        body={maasBodyTemplate}
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="iseBaslamaTarihi"
                        header={(
                            <div className="flex align-items-center gap-1">
                                <span>İşe Başlama</span>
                                <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                            </div>
                        )}
                        body={iseBaslamaBodyTemplate}
                        sortable
                        filter
                        filterElement={dateFilterTemplate}
                        filterField="iseBaslamaTarihi"
                        dataType="date"
                        filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                        style={{ minWidth: '12rem' }}
                    ></Column>
                    <Column
                        field="kalanIzinHakki"
                        header="Kalan İzin Hakkı"
                        body={izinHakkiBodyTemplate}
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        field="aktif"
                        header={(
                            <div className="flex align-items-center gap-1">
                                <span>Durum</span>
                                <i className="pi pi-filter text-400" style={{ fontSize: '0.7rem' }}></i>
                            </div>
                        )}
                        body={statusBodyTemplate}
                        sortable
                        filter
                        filterElement={statusFilterTemplate}
                        filterHeaderStyle={{ padding: '0.25rem', height: '2.2rem' }}
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        body={actionBodyTemplate}
                        header="İşlemler"
                        style={{ minWidth: '8rem' }}
                        frozen
                        alignFrozen="right"
                    ></Column>
                </DataTable>
            </Card>

            <Dialog
                visible={personelDialog}
                style={{ width: '1200px' }}
                header="Personel Detayları"
                modal
                className="p-fluid"
                footer={personelDialogFooter}
                onHide={hideDialog}
                maximizable
            >
                <TabView activeIndex={activeTabIndex} onTabChange={(e) => setActiveTabIndex(e.index)}>
                    {/* Genel Bilgiler Sekmesi */}
                    <TabPanel header="Genel Bilgiler" leftIcon="pi pi-user">
                        <div className="p-formgrid p-grid">
                            {/* Sol Kolon */}
                            <div className="p-field p-col-6">
                                <Fieldset legend="Kimlik ve Sistem Bilgileri" className="p-mb-4">
                                    {/* Sicil Numarası (sadece güncelleme için) */}
                                    {personel.id && (
                                        <div className="p-field">
                                            <label htmlFor="sicilNo">Sicil Numarası</label>
                                            <InputText
                                                id="sicilNo"
                                                value={personel.id || ''}
                                                disabled
                                                className="p-inputtext-sm"
                                            />
                                        </div>
                                    )}
                                    
                                    {/* Kullanıcı Adı (sadece güncelleme için) */}
                                    {personel.id && (
                                        <div className="p-field">
                                            <label htmlFor="kullaniciAdi">Kullanıcı Adı</label>
                                            <InputText
                                                id="kullaniciAdi"
                                                value={personel.kullaniciAdi || ''}
                                                onChange={(e) => onInputChange(e, 'kullaniciAdi')}
                                                placeholder="kullanici.adi"
                                                className="p-inputtext-sm"
                                            />
                                            <small className="p-text-secondary">
                                                Sadece İngilizce harfler, sayılar, nokta (.) ve alt çizgi (_) kullanın.
                                            </small>
                                        </div>
                                    )}
                                    
                                    <div className="p-field">
                                        <label htmlFor="tcKimlik">TC Kimlik No *</label>
                                        <InputMask
                                            id="tcKimlik"
                                            mask="99999999999"
                                            value={personel.tcKimlik}
                                            onChange={(e) => onInputChange(e, 'tcKimlik')}
                                            required
                                            className={submitted && personel.tcKimlik.length !== 11 ? 'p-invalid' : ''}
                                        />
                                        {submitted && personel.tcKimlik.length !== 11 && (
                                            <small className="p-error">TC Kimlik numarası 11 haneli olmalıdır.</small>
                                        )}
                                    </div>
                                </Fieldset>

                                <Fieldset legend="Kişisel Bilgiler" className="p-mb-4">
                                    <div className="p-formgrid p-grid">
                                        <div className="p-field p-col-6">
                                            <label htmlFor="ad">Ad *</label>
                                            <InputText
                                                id="ad"
                                                value={personel.ad}
                                                onChange={(e) => onInputChange(e, 'ad')}
                                                required
                                                className={submitted && !personel.ad ? 'p-invalid' : ''}
                                            />
                                            {submitted && !personel.ad && (
                                                <small className="p-error">Ad gereklidir.</small>
                                            )}
                                        </div>
                                        <div className="p-field p-col-6">
                                            <label htmlFor="soyad">Soyad *</label>
                                            <InputText
                                                id="soyad"
                                                value={personel.soyad}
                                                onChange={(e) => onInputChange(e, 'soyad')}
                                                required
                                                className={submitted && !personel.soyad ? 'p-invalid' : ''}
                                            />
                                            {submitted && !personel.soyad && (
                                                <small className="p-error">Soyad gereklidir.</small>
                                            )}
                                        </div>
                                    </div>
                                    <div className="p-field">
                                        <label htmlFor="dogumTarihi">Doğum Tarihi</label>
                                        <Calendar
                                            id="dogumTarihi"
                                            value={personel.dogumTarihi}
                                            onChange={(e) => onDateChange(e, 'dogumTarihi')}
                                            dateFormat="dd/mm/yy"
                                            firstDayOfWeek={1}
                                            placeholder="dd/mm/yyyy"
                                            showIcon
                                        />
                                    </div>
                                </Fieldset>
                            </div>

                            {/* Sağ Kolon - Fotoğraf ve İletişim */}
                            <div className="p-field p-col-6">
                                <Fieldset legend="Fotoğraf" className="p-mb-4">
                                    <div className="p-d-flex p-ai-center p-jc-center">
                                        {(uploadedFile || personel.fotografUrl) ? (
                                            <div className="p-text-center">
                                                <Avatar
                                                    image={uploadedFile ? fileUploadService.getAvatarUrl(uploadedFile.fileName) : fileUploadService.getAvatarUrl(personel.fotografUrl)}
                                                    size="xlarge"
                                                    shape="circle"
                                                />
                                                <div className="p-mt-2">
                                                    <Button
                                                        icon="pi pi-times"
                                                        className="p-button-rounded p-button-danger p-button-sm"
                                                        onClick={removeUploadedFile}
                                                        tooltip="Fotoğrafı Kaldır"
                                                    />
                                                </div>
                                            </div>
                                        ) : (
                                            <FileUpload
                                                ref={fileUploadRef}
                                                mode="basic"
                                                accept="image/*"
                                                maxFileSize={5000000}
                                                customUpload
                                                uploadHandler={onFileUpload}
                                                auto
                                                chooseLabel="Fotoğraf Seç"
                                                className="p-button-outlined"
                                            />
                                        )}
                                    </div>
                                </Fieldset>

                                <Fieldset legend="İletişim Bilgileri" className="p-mb-4">
                                    <div className="p-field">
                                        <label htmlFor="email">Email</label>
                                        <InputText
                                            id="email"
                                            type="email"
                                            value={personel.email}
                                            onChange={(e) => onInputChange(e, 'email')}
                                        />
                                    </div>
                                    <div className="p-field">
                                        <label htmlFor="telefon">Telefon</label>
                                        <InputMask
                                            id="telefon"
                                            mask="9999-999-9999"
                                            value={personel.telefon}
                                            onChange={(e) => onInputChange(e, 'telefon')}
                                        />
                                    </div>
                                    <div className="p-field">
                                        <label htmlFor="adres">Adres</label>
                                        <InputTextarea
                                            id="adres"
                                            value={personel.adres || ''}
                                            onChange={(e) => onInputChange(e, 'adres')}
                                            rows={3}
                                        />
                                    </div>
                                </Fieldset>
                            </div>
                        </div>
                    </TabPanel>

                    {/* Pozisyon Bilgileri Sekmesi */}
                    <TabPanel header="Pozisyon Bilgileri" leftIcon="pi pi-briefcase">
                        <div className="p-formgrid p-grid">
                            <div className="p-field p-col-6">
                                <Fieldset legend="Organizasyon Bilgileri" className="p-mb-4">
                                    <div className="p-field">
                                        <label htmlFor="departman">Departman *</label>
                                        <Dropdown
                                            id="departman"
                                            value={selectedDepartmanId}
                                            options={departmanlar}
                                            onChange={onDepartmanChange}
                                            optionLabel="ad"
                                            optionValue="id"
                                            placeholder="Departman seçiniz"
                                            className={submitted && !personel.pozisyonId ? 'p-invalid' : ''}
                                        />
                                    </div>
                                    <div className="p-field">
                                        <label htmlFor="pozisyonId">Pozisyon *</label>
                                        <Dropdown
                                            id="pozisyonId"
                                            value={personel.pozisyonId}
                                            options={pozisyonlarFiltered}
                                            onChange={onPozisyonChange}
                                            optionLabel="ad"
                                            optionValue="id"
                                            placeholder="Pozisyon seçiniz"
                                            disabled={!selectedDepartmanId}
                                            className={submitted && !personel.pozisyonId ? 'p-invalid' : ''}
                                        />
                                        {submitted && !personel.pozisyonId && (
                                            <small className="p-error">Pozisyon seçimi gereklidir.</small>
                                        )}
                                    </div>
                                    <div className="p-field">
                                        <label htmlFor="yoneticiId">Yönetici</label>
                                        <Dropdown
                                            id="yoneticiId"
                                            value={personel.yoneticiId}
                                            options={yoneticilerFiltered}
                                            onChange={(e) => onDropdownChange(e, 'yoneticiId')}
                                            optionLabel="adSoyad"
                                            optionValue="id"
                                            itemTemplate={yoneticiItemTemplate}
                                            placeholder="Yönetici seçiniz"
                                            disabled={!personel.pozisyonId}
                                        />
                                    </div>
                                </Fieldset>

                                <Fieldset legend="İş Tarihleri" className="p-mb-4">
                                    <div className="p-field">
                                        <label htmlFor="iseBaslamaTarihi">İşe Başlama Tarihi *</label>
                                        <Calendar
                                            id="iseBaslamaTarihi"
                                            value={personel.iseBaslamaTarihi}
                                            onChange={(e) => onDateChange(e, 'iseBaslamaTarihi')}
                                            dateFormat="dd/mm/yy"
                                            firstDayOfWeek={1}
                                            placeholder="dd/mm/yyyy"
                                            showIcon
                                            required
                                            className={submitted && !personel.iseBaslamaTarihi ? 'p-invalid' : ''}
                                        />
                                        {submitted && !personel.iseBaslamaTarihi && (
                                            <small className="p-error">İşe başlama tarihi gereklidir.</small>
                                        )}
                                    </div>
                                    <div className="p-field">
                                        <label htmlFor="cikisTarihi">Çıkış Tarihi</label>
                                        <Calendar
                                            id="cikisTarihi"
                                            value={personel.cikisTarihi}
                                            onChange={(e) => onDateChange(e, 'cikisTarihi')}
                                            dateFormat="dd/mm/yy"
                                            firstDayOfWeek={1}
                                            placeholder="dd/mm/yyyy"
                                            showIcon
                                        />
                                    </div>
                                </Fieldset>
                            </div>

                            <div className="p-field p-col-6">
                                <Fieldset legend="Maaş Bilgileri" className="p-mb-4">
                                    {pozisyonMaasBilgi && (
                                        <div className="p-field">
                                            <Message
                                                severity="info"
                                                text={`${pozisyonMaasBilgi.ad} pozisyonu maaş aralığı: ₺${pozisyonMaasBilgi.minMaas?.toLocaleString() || '0'} - ₺${pozisyonMaasBilgi.maxMaas?.toLocaleString() || '∞'}`}
                                            />
                                        </div>
                                    )}
                                    <div className="p-field">
                                        <label htmlFor="maas">Maaş (₺)</label>
                                        <InputNumber
                                            id="maas"
                                            value={personel.maas}
                                            onValueChange={onMaasChange}
                                            mode="currency"
                                            currency="TRY"
                                            locale="tr"
                                            currencyDisplay="code"
                                            placeholder="0,00"
                                        />
                                    </div>
                                    {maasUyarisi && (
                                        <div className="p-field">
                                            <Message severity="warn" text={maasUyarisi} />
                                        </div>
                                    )}
                                </Fieldset>

                                <Fieldset legend="Durum" className="p-mb-4">
                                    <div className="p-field">
                                        <label htmlFor="aktif">Personel Durumu</label>
                                        <div className="p-mt-2">
                                            <InputSwitch
                                                id="aktif"
                                                checked={personel.aktif}
                                                onChange={(e) => setPersonel({ ...personel, aktif: e.value })}
                                            />
                                            <span className="p-ml-2">
                                                {personel.aktif ? 'Aktif' : 'Pasif'}
                                            </span>
                                        </div>
                                    </div>
                                </Fieldset>
                            </div>
                        </div>
                    </TabPanel>

                    {/* Özlük Bilgileri Sekmesi */}
                    <TabPanel header="Özlük Bilgileri" leftIcon="pi pi-id-card">
                        <div className="p-formgrid p-grid">
                            <div className="p-field p-col-6">
                                <Fieldset legend="Kisisel Bilgiler" className="p-mb-4">
                                    <div className="p-formgrid p-grid">
                                        <div className="p-field p-col-6">
                                            <label htmlFor="medeniHal">Medeni Hal</label>
                                            <Dropdown
                                                id="medeniHal"
                                                value={personel.medeniHal}
                                                options={medeniHalOptions}
                                                onChange={(e) => onDropdownChange(e, 'medeniHal')}
                                                optionLabel="label"
                                                optionValue="value"
                                                placeholder="Medeni hal seçiniz"
                                            />
                                        </div>
                                        <div className="p-field p-col-6">
                                            <label htmlFor="cinsiyet">Cinsiyet</label>
                                            <Dropdown
                                                id="cinsiyet"
                                                value={personel.cinsiyet}
                                                options={cinsiyetOptions}
                                                onChange={(e) => onDropdownChange(e, 'cinsiyet')}
                                                optionLabel="label"
                                                optionValue="value"
                                                placeholder="Cinsiyet seçiniz"
                                            />
                                        </div>
                                    </div>
                                    <div className="p-field">
                                        <label htmlFor="dogumYeri">Doğum Yeri</label>
                                        <InputText
                                            id="dogumYeri"
                                            value={personel.dogumYeri}
                                            onChange={(e) => onInputChange(e, 'dogumYeri')}
                                            placeholder="Doğum yeri giriniz"
                                        />
                                    </div>
                                    <div className="p-field">
                                        <label htmlFor="kanGrubu">Kan Grubu</label>
                                        <Dropdown
                                            id="kanGrubu"
                                            value={personel.kanGrubu}
                                            options={[
                                                { label: 'A+', value: 'A+' },
                                                { label: 'A-', value: 'A-' },
                                                { label: 'B+', value: 'B+' },
                                                { label: 'B-', value: 'B-' },
                                                { label: 'AB+', value: 'AB+' },
                                                { label: 'AB-', value: 'AB-' },
                                                { label: '0+', value: '0+' },
                                                { label: '0-', value: '0-' }
                                            ]}
                                            onChange={(e) => onDropdownChange(e, 'kanGrubu')}
                                            optionLabel="label"
                                            optionValue="value"
                                            placeholder="Kan grubu seçiniz"
                                        />
                                    </div>
                                </Fieldset>

                                <Fieldset legend="Aile Bilgileri" className="p-mb-4">
                                    <div className="p-field">
                                        <label htmlFor="anneAdi">Anne Adı</label>
                                        <InputText
                                            id="anneAdi"
                                            value={personel.anneAdi}
                                            onChange={(e) => onInputChange(e, 'anneAdi')}
                                            placeholder="Anne adını giriniz"
                                        />
                                    </div>
                                    <div className="p-field">
                                        <label htmlFor="babaAdi">Baba Adı</label>
                                        <InputText
                                            id="babaAdi"
                                            value={personel.babaAdi}
                                            onChange={(e) => onInputChange(e, 'babaAdi')}
                                            placeholder="Baba adını giriniz"
                                        />
                                    </div>
                                </Fieldset>
                            </div>

                            <div className="p-field p-col-6">
                                <Fieldset legend="Eğitim ve Askerlik" className="p-mb-4">
                                    <div className="p-field">
                                        <label htmlFor="egitimDurumu">Eğitim Durumu</label>
                                        <Dropdown
                                            id="egitimDurumu"
                                            value={personel.egitimDurumu}
                                            options={[
                                                { label: 'İlkokul', value: 'Ilkokul' },
                                                { label: 'Ortaokul', value: 'Ortaokul' },
                                                { label: 'Lise', value: 'Lise' },
                                                { label: 'Ön Lisans', value: 'OnLisans' },
                                                { label: 'Lisans', value: 'Lisans' },
                                                { label: 'Yüksek Lisans', value: 'YuksekLisans' },
                                                { label: 'Doktora', value: 'Doktora' }
                                            ]}
                                            onChange={(e) => onDropdownChange(e, 'egitimDurumu')}
                                            optionLabel="label"
                                            optionValue="value"
                                            placeholder="Eğitim durumu seçiniz"
                                        />
                                    </div>
                                    <div className="p-field">
                                        <label htmlFor="askerlikDurumu">Askerlik Durumu</label>
                                        <Dropdown
                                            id="askerlikDurumu"
                                            value={personel.askerlikDurumu}
                                            options={[
                                                { label: 'Yapılmadı', value: 'Yapilmadi' },
                                                { label: 'Yapıldı', value: 'Yapildi' },
                                                { label: 'Muaf', value: 'Muaf' },
                                                { label: 'Tecilli', value: 'Tecilli' }
                                            ]}
                                            onChange={(e) => onDropdownChange(e, 'askerlikDurumu')}
                                            optionLabel="label"
                                            optionValue="value"
                                            placeholder="Askerlik durumu seçiniz"
                                        />
                                    </div>
                                    <div className="p-field">
                                        <label htmlFor="ehliyetSinifi">Ehliyet Sınıfı</label>
                                        <InputText
                                            id="ehliyetSinifi"
                                            value={personel.ehliyetSinifi}
                                            onChange={(e) => onInputChange(e, 'ehliyetSinifi')}
                                            placeholder="A, B, C vb."
                                        />
                                    </div>
                                </Fieldset>

                                <Fieldset legend="Nüfus Bilgileri" className="p-mb-4">
                                    <div className="p-formgrid p-grid">
                                        <div className="p-field p-col-6">
                                            <label htmlFor="nufusIlKod">Nüfus İl Kodu</label>
                                            <InputText
                                                id="nufusIlKod"
                                                value={personel.nufusIlKod}
                                                onChange={(e) => onInputChange(e, 'nufusIlKod')}
                                                placeholder="İl kodu"
                                            />
                                        </div>
                                        <div className="p-field p-col-6">
                                            <label htmlFor="nufusIlceKod">Nüfus İlçe Kodu</label>
                                            <InputText
                                                id="nufusIlceKod"
                                                value={personel.nufusIlceKod}
                                                onChange={(e) => onInputChange(e, 'nufusIlceKod')}
                                                placeholder="İlçe kodu"
                                            />
                                        </div>
                                    </div>
                                </Fieldset>
                            </div>
                        </div>
                    </TabPanel>

                    {/* Acil Durum & Banka Sekmesi */}
                    <TabPanel header="Acil Durum & Banka" leftIcon="pi pi-phone">
                        <div className="p-formgrid p-grid">
                            <div className="p-field p-col-6">
                                <Fieldset legend="Acil Durum İletişimi" className="p-mb-4">
                                    <div className="p-field">
                                        <label htmlFor="acilDurumIletisim">Acil Durum İletişim Bilgileri</label>
                                        <InputTextarea
                                            id="acilDurumIletisim"
                                            value={personel.acilDurumIletisim}
                                            onChange={(e) => onInputChange(e, 'acilDurumIletisim')}
                                            rows={4}
                                            placeholder="Ad Soyad, Yakınlık, Telefon"
                                        />
                                        <small className="p-text-secondary">
                                            Örnek: Ahmet Yılmaz, Eş, 0555-123-4567
                                        </small>
                                    </div>
                                </Fieldset>
                            </div>

                            <div className="p-field p-col-6">
                                <Fieldset legend="Banka Bilgileri" className="p-mb-4">
                                    <div className="p-field">
                                        <label htmlFor="bankaHesapNo">Banka Hesap No</label>
                                        <InputText
                                            id="bankaHesapNo"
                                            value={personel.bankaHesapNo}
                                            onChange={(e) => onInputChange(e, 'bankaHesapNo')}
                                            placeholder="Hesap numarası"
                                        />
                                    </div>
                                    <div className="p-field">
                                        <label htmlFor="ibanNo">IBAN No</label>
                                        <InputMask
                                            id="ibanNo"
                                            mask="TR99 9999 9999 9999 9999 9999 99"
                                            value={personel.ibanNo}
                                            onChange={(e) => onInputChange(e, 'ibanNo')}
                                            placeholder="TR00 0000 0000 0000 0000 0000 00"
                                        />
                                    </div>
                                </Fieldset>
                            </div>
                        </div>
                    </TabPanel>
                </TabView>
            </Dialog>
        </div>
    );
};

export default Personeller;