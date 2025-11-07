import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { InputTextarea } from 'primereact/inputtextarea';
import { Dropdown } from 'primereact/dropdown';
import { Calendar } from 'primereact/calendar';
import { Badge } from 'primereact/badge';
import { Avatar } from 'primereact/avatar';
import { confirmDialog } from 'primereact/confirmdialog';
import { Card } from 'primereact/card';
import { Divider } from 'primereact/divider';
import { Message } from 'primereact/message';
import { Panel } from 'primereact/panel';
import { Chip } from 'primereact/chip';
import { FileUpload } from 'primereact/fileupload';
import izinService from '../services/izinService';
import fileUploadService from '../services/fileUploadService';
import authService from '../services/authService';
import yetkiService from '../services/yetkiService';
import izinKonfigurasyonService from '../services/izinKonfigurasyonService';

const IzinTalepleri = () => {
    const [izinTalepleri, setIzinTalepleri] = useState([]);
    const [izinDialog, setIzinDialog] = useState(false);
    const [onayDialog, setOnayDialog] = useState(false);
    const [izinTalebi, setIzinTalebi] = useState({
        id: null,
        personelId: null,
        izinBaslamaTarihi: null,
        isbasiTarihi: null,
        izinBaslamaSaati: '08:00',
        isbasiSaati: '08:00',
        gunSayisi: 0,
        izinTipi: 'Yıllık İzin',
        gorevYeri: '',
        aciklama: ''
    });
    const [selectedIzin, setSelectedIzin] = useState(null);
    const [onayNotu, setOnayNotu] = useState('');
    const [onayTipi, setOnayTipi] = useState('onayla'); // 'onayla' veya 'reddet'
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
    const [izinOzeti, setIzinOzeti] = useState(null);
    const [currentUser, setCurrentUser] = useState(null);
    const [izinTipleri, setIzinTipleri] = useState([]);
    const [uploadedRapor, setUploadedRapor] = useState(null);
    // Permission states
    const [permissions, setPermissions] = useState({
        read: false,
        write: false,
        delete: false,
        update: false
    });

    const toast = useRef(null);
    const dt = useRef(null);

    useEffect(() => {
        const user = authService.getUser();
        setCurrentUser(user);
        loadData();
        loadPermissions();
        loadIzinTipleri();
    }, []);

    const loadIzinTipleri = async () => {
        try {
            const user = authService.getUser();
            let response;

            // Cinsiyet bilgisi varsa, cinsiyet bazlı filtreleme yap
            if (user && user.personel && user.personel.cinsiyet) {
                response = await izinKonfigurasyonService.getIzinTipleriByGender(user.personel.cinsiyet);
            } else {
                // Cinsiyet bilgisi yoksa tüm aktif izin tiplerini getir
                response = await izinKonfigurasyonService.getAktifIzinTipleri();
            }

            if (response.success) {
                // Personelin izin özetini al (toplamHak için)
                const izinOzetiResponse = await izinService.getPersonelIzinOzeti(user.personel.id);
                const toplamHak = izinOzetiResponse.success ? izinOzetiResponse.data.toplamHak : null;

                // Transform API data to dropdown format
                const dropdownOptions = response.data.map(tip => {
                    const maksimum = tip.izinTipiAdi === 'Yıllık İzin' ? toplamHak : tip.maksimumGunSayisi;

                    return {
                        label: tip.izinTipiAdi,
                        value: tip.izinTipiAdi,
                        minimumGunSayisi: tip.minimumGunSayisi,
                        // Yıllık İzin için maksimum = personelin toplam hakkı
                        maksimumGunSayisi: maksimum,
                        raporGerekli: tip.raporGerekli
                    };
                });
                setIzinTipleri(dropdownOptions);
            }
        } catch (error) {
            console.error('İzin tipleri yüklenirken hata:', error);
            // Fallback to default values if API fails
            setIzinTipleri([
                { label: 'Yıllık İzin', value: 'Yıllık İzin' },
                { label: 'Mazeret İzni', value: 'Mazeret İzni' },
                { label: 'Hastalık İzni', value: 'Hastalık İzni' },
                { label: 'Doğum İzni', value: 'Doğum İzni' },
                { label: 'Ücretsiz İzin', value: 'Ücretsiz İzin' },
                { label: 'Dış Görev', value: 'Dış Görev' },
                { label: 'Diğer', value: 'Diğer' }
            ]);
        }
    };

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('izin-talepleri', 'read'),
                write: yetkiService.hasScreenPermission('izin-talepleri', 'write'),
                delete: yetkiService.hasScreenPermission('izin-talepleri', 'delete'),
                update: yetkiService.hasScreenPermission('izin-talepleri', 'update')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
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
        const user = authService.getUser();
        if (!user) return;

        await Promise.all([
            loadIzinTalepleri(),
            loadIzinOzeti(user.personel.id)
        ]);
    };

    const loadIzinTalepleri = async () => {
        setLoading(true);
        try {
            const user = authService.getUser();
            if (!user || !user.personel) {
                throw new Error('Kullanıcı bilgileri bulunamadı');
            }

            // Sadece kullanıcının kendi izin taleplerini getir
            const response = await izinService.getAllIzinTalepleri(user.personel.id);

            if (response.success) {
                setIzinTalepleri(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'İzin talepleri yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const loadIzinOzeti = async (personelId) => {
        try {
            const response = await izinService.getPersonelIzinOzeti(personelId);
            if (response.success) {
                setIzinOzeti(response.data);
            }
        } catch (error) {
            // console.error('İzin özeti yüklenirken hata:', error);
        }
    };

    const openNew = () => {
        const user = authService.getUser();
        setIzinTalebi({
            id: null,
            personelId: user.personel.id,
            izinBaslamaTarihi: null,
            isbasiTarihi: null,
            izinBaslamaSaati: '08:00',
            isbasiSaati: '08:00',
            gunSayisi: 0,
            izinTipi: 'Yıllık İzin',
            aciklama: ''
        });
        setUploadedRapor(null);
        setSubmitted(false);
        setIzinDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setIzinDialog(false);
    };

    const hideOnayDialog = () => {
        setOnayDialog(false);
        setOnayNotu('');
        setSelectedIzin(null);
    };

    const saveIzinTalebi = async () => {
        setSubmitted(true);

        // Tarih kontrolü - önce tarihlerin seçilmiş olması gerekir
        if (!izinTalebi.izinBaslamaTarihi || !izinTalebi.isbasiTarihi) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'İzin başlama ve işbaşı tarihlerini seçmelisiniz.',
                life: 3000
            });
            return;
        }

        // Gün sayısı kontrolü - 0 veya negatif olamaz
        if (!izinTalebi.gunSayisi || izinTalebi.gunSayisi <= 0) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'İzin talebi en az 1 iş günü olmalıdır. Lütfen tarihleri kontrol edin.',
                life: 3000
            });
            return;
        }

        // Dış Görev için görev yeri validasyonu
        if (izinTalebi.izinTipi === 'Dış Görev' && !izinTalebi.gorevYeri) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Dış görev için görev yeri girilmelidir.',
                life: 3000
            });
            return;
        }

        // İzin tipi min/max gün sayısı ve rapor kontrolü
        const selectedIzinTipi = izinTipleri.find(tip => tip.value === izinTalebi.izinTipi);

        if (selectedIzinTipi) {
            // Minimum gün kontrolü
            if (selectedIzinTipi.minimumGunSayisi &&
                selectedIzinTipi.minimumGunSayisi > 0 &&
                izinTalebi.gunSayisi < selectedIzinTipi.minimumGunSayisi) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: `${izinTalebi.izinTipi} için minimum ${selectedIzinTipi.minimumGunSayisi} gün talep etmelisiniz.`,
                    life: 3000
                });
                return;
            }

            // Maksimum gün kontrolü (0'dan büyük herhangi bir değer varsa kontrol yap)
            if (selectedIzinTipi.maksimumGunSayisi &&
                selectedIzinTipi.maksimumGunSayisi > 0 &&
                izinTalebi.gunSayisi > selectedIzinTipi.maksimumGunSayisi) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: `${izinTalebi.izinTipi} için maksimum ${selectedIzinTipi.maksimumGunSayisi} gün talep edebilirsiniz. Girilen gün: ${izinTalebi.gunSayisi}`,
                    life: 5000
                });
                return;
            }

            // Rapor gereklilik kontrolü
            if (selectedIzinTipi.raporGerekli && !uploadedRapor) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: `${izinTalebi.izinTipi} için rapor yüklemeniz gerekmektedir.`,
                    life: 3000
                });
                return;
            }
        } else {
            // İzin tipi bulunamadı hatası
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'İzin tipi bilgisi bulunamadı. Lütfen sayfayı yenileyip tekrar deneyin.',
                life: 3000
            });
            return;
        }

        if (izinTalebi.izinBaslamaTarihi && izinTalebi.isbasiTarihi &&
            izinTalebi.izinBaslamaTarihi < izinTalebi.isbasiTarihi) {

            try {
                let response;
                if (izinTalebi.id) {
                    response = await izinService.updateIzinTalebi(izinTalebi.id, izinTalebi);
                } else {
                    // Debug: Rapor bilgilerini kontrol et
                    console.log('DEBUG - uploadedRapor:', uploadedRapor);
                    console.log('DEBUG - raporYuklenecek:', uploadedRapor != null);
                    console.log('DEBUG - izinTalebi.izinTipi:', izinTalebi.izinTipi);

                    // Rapor yüklenecek mi bilgisini backend'e gönder
                    const talepData = {
                        ...izinTalebi,
                        raporYuklenecek: uploadedRapor != null
                    };

                    console.log('DEBUG - Backend\'e gönderilen data:', talepData);
                    response = await izinService.createIzinTalebi(talepData);
                    console.log('DEBUG - Backend response:', response);
                }

                if (response.success) {
                    // Rapor yükleme işlemi (izin talebi oluşturulduktan sonra)
                    if (uploadedRapor && response.data && response.data.id) {
                        try {
                            const uploadResponse = await izinService.uploadRapor(response.data.id, uploadedRapor);
                            if (!uploadResponse.success) {
                                toast.current.show({
                                    severity: 'warn',
                                    summary: 'Uyarı',
                                    detail: 'İzin talebi oluşturuldu ancak rapor yüklenemedi.',
                                    life: 3000
                                });
                            }
                        } catch (uploadError) {
                            console.error('Rapor yükleme hatası:', uploadError);
                        }
                    }

                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: response.message,
                        life: 3000
                    });
                    loadData();
                    setIzinDialog(false);
                } else {
                    // Backend'den success=false gelirse
                    console.error('DEBUG - Backend hatası:', response.message);
                    toast.current.show({
                        severity: 'error',
                        summary: 'Hata',
                        detail: response.message,
                        life: 5000
                    });
                }
            } catch (error) {
                console.error('DEBUG - Exception:', error);
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: error.message,
                    life: 5000
                });
            }
        }
    };

    const editIzinTalebi = (izin) => {
        setIzinTalebi({
            ...izin,
            izinBaslamaTarihi: new Date(izin.izinBaslamaTarihi || izin.baslangicTarihi),
            isbasiTarihi: new Date(izin.isbasiTarihi || izin.bitisTarihi),
            izinBaslamaSaati: '08:00',
            isbasiSaati: '08:00'
        });
        setIzinDialog(true);
    };

    const confirmDeleteIzin = (izin) => {
        confirmDialog({
            message: 'Bu izin talebini silmek istediğinizden emin misiniz?',
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deleteIzinTalebi(izin.id)
        });
    };

    const deleteIzinTalebi = async (id) => {
        try {
            const response = await izinService.deleteIzinTalebi(id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadData();
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

    const openOnayDialog = (izin, tip) => {
        setSelectedIzin(izin);
        setOnayTipi(tip);
        setOnayDialog(true);
    };

    const processOnayReddet = async () => {
        if (!selectedIzin) return;

        try {
            const user = authService.getUser();
            let response;

            if (onayTipi === 'onayla') {
                response = await izinService.onaylaIzinTalebi(selectedIzin.id, user.personel.id, onayNotu);
            } else {
                response = await izinService.reddetIzinTalebi(selectedIzin.id, user.personel.id, onayNotu);
            }

            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadData();
                hideOnayDialog();
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

    const calculateGunSayisi = (talebi) => {
        const _izinTalebi = talebi || izinTalebi;

        if (_izinTalebi.izinBaslamaTarihi && _izinTalebi.isbasiTarihi) {
            const start = new Date(_izinTalebi.izinBaslamaTarihi);
            const end = new Date(_izinTalebi.isbasiTarihi);
            let gunSayisi = 0;

            // İzin başlama ve işbaşı tarihleri arasındaki tüm günleri hesapla (işbaşı günü hariç)
            const current = new Date(start);
            while (current < end) { // İşbaşı tarihi dahil değil
                if (current.getDay() !== 0 && current.getDay() !== 6) { // Hafta sonu hariç
                    // Bu gün tam izin günü mü yoksa yarım mı?
                    if (current.getTime() === start.getTime()) {
                        // İzin başlama günü
                        const izinSaat = parseInt(_izinTalebi.izinBaslamaSaati?.split(':')[0] || '8');
                        gunSayisi += izinSaat >= 13 ? 0.5 : 1;
                    } else {
                        // Ara günler tam gün izin
                        gunSayisi += 1;
                    }
                }
                current.setDate(current.getDate() + 1);
            }

            return gunSayisi;
        }

        return 0;
    };

    const onInputChange = (e, name) => {
        const val = (e.target && e.target.value) || '';
        let _izinTalebi = { ...izinTalebi };
        _izinTalebi[`${name}`] = val;
        setIzinTalebi(_izinTalebi);
    };

    const onDateChange = (e, name) => {
        const val = e.value;
        let _izinTalebi = { ...izinTalebi };
        _izinTalebi[`${name}`] = val;

        // Her iki tarih de doluysa gün sayısını hemen hesapla
        if (_izinTalebi.izinBaslamaTarihi && _izinTalebi.isbasiTarihi) {
            const hesaplananGun = calculateGunSayisi(_izinTalebi);
            _izinTalebi.gunSayisi = hesaplananGun;
        }

        setIzinTalebi(_izinTalebi);
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                {permissions.write && (
                    <Button
                        label="Yeni İzin Talebi"
                        icon="pi pi-plus"
                        className="p-button-success p-mr-2"
                        onClick={openNew}
                    />
                )}
            </React.Fragment>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <React.Fragment>
                <Button
                    label="Dışa Aktar"
                    icon="pi pi-upload"
                    className="p-button-help"
                    onClick={() => dt.current.exportCSV()}
                />
            </React.Fragment>
        );
    };

    const actionBodyTemplate = (rowData) => {
        const user = authService.getUser();
        const isOwner = rowData.personelId === user.personel.id;
        const canEdit = rowData.durum === 'Beklemede' && isOwner && permissions.update;
        const canDelete = rowData.durum === 'Beklemede' && isOwner && permissions.delete;

        return (
            <React.Fragment>
                {canEdit && (
                    <Button
                        icon="pi pi-pencil"
                        className="p-button-rounded p-button-success p-button-sm p-mr-1"
                        onClick={() => editIzinTalebi(rowData)}
                        tooltip="Düzenle"
                    />
                )}
                {canDelete && (
                    <Button
                        icon="pi pi-trash"
                        className="p-button-rounded p-button-warning p-button-sm p-mr-1"
                        onClick={() => confirmDeleteIzin(rowData)}
                        tooltip="Sil"
                    />
                )}
                {!canEdit && !canDelete && (
                    <span className="text-500">-</span>
                )}
            </React.Fragment>
        );
    };

    const avatarBodyTemplate = (rowData) => {
        if (rowData.personelFotograf) {
            return (
                <Avatar
                    image={fileUploadService.getAvatarUrl(rowData.personelFotograf)}
                    size="normal"
                    shape="circle"
                />
            );
        } else {
            const names = rowData.personelAd.split(' ');
            return (
                <Avatar
                    label={names[0].charAt(0) + (names[1] ? names[1].charAt(0) : '')}
                    size="normal"
                    shape="circle"
                    style={{ backgroundColor: '#2196F3', color: '#ffffff' }}
                />
            );
        }
    };

    const durumBodyTemplate = (rowData) => {
        return (
            <Badge
                value={rowData.durum}
                severity={izinService.getDurumRengi(rowData.durum)}
            />
        );
    };

    const izinTipiBodyTemplate = (rowData) => {
        return (
            <Badge
                value={rowData.izinTipi}
                severity={izinService.getIzinTipiRengi(rowData.izinTipi)}
            />
        );
    };

    const tarihBodyTemplate = (field) => (rowData) => {
        // Hem yeni hem eski alan adlarını destekle
        let tarihDegeri = rowData[field];
        if (!tarihDegeri) {
            // Fallback eski alan adları
            if (field === 'izinBaslamaTarihi') {
                tarihDegeri = rowData.baslangicTarihi;
            } else if (field === 'isbasiTarihi') {
                tarihDegeri = rowData.bitisTarihi;
            }
        }
        return izinService.formatTarih(tarihDegeri);
    };

    const header = (
        <div className="table-header">
            <h5 className="p-m-0">İzin Taleplerim</h5>
            <span className="p-input-icon-left">
                <i className="pi pi-search" />
                <InputText
                    type="search"
                    onInput={(e) => setGlobalFilter(e.target.value)}
                    placeholder="Arama yapın..."
                />
            </span>
        </div>
    );

    const izinDialogFooter = (
        <React.Fragment>
            <Button
                label="İptal"
                icon="pi pi-times"
                className="p-button-text"
                onClick={hideDialog}
            />
            <Button
                label="Yönetici Onayına Gönder"
                icon="pi pi-send"
                className="p-button-success"
                onClick={saveIzinTalebi}
            />
        </React.Fragment>
    );

    const onayDialogFooter = (
        <React.Fragment>
            <Button
                label="İptal"
                icon="pi pi-times"
                className="p-button-text"
                onClick={hideOnayDialog}
            />
            <Button
                label={onayTipi === 'onayla' ? 'Onayla' : 'Reddet'}
                icon={onayTipi === 'onayla' ? 'pi pi-check' : 'pi pi-times'}
                className={onayTipi === 'onayla' ? 'p-button-success' : 'p-button-danger'}
                onClick={processOnayReddet}
            />
        </React.Fragment>
    );

    return (
        <div className="datatable-crud-demo">
            <Toast ref={toast} />

            {/* İzin Özeti Kartı */}
            {izinOzeti && (
                <Card className="p-mb-4">
                    <div className="p-d-flex p-jc-between p-ai-center">
                        <h6>İzin Haklarınız ({izinOzeti.yil})</h6>
                        <div className="p-d-flex p-ai-center">
                            <Chip label={`Toplam: ${izinOzeti.toplamHak} gün`} className="p-mr-2" />
                            <Chip label={`Kullanılan: ${izinOzeti.kullanilmis} gün`} className="p-mr-2 p-chip-warning" />
                            <Chip label={`Bekleyen: ${izinOzeti.bekleyen} gün`} className="p-mr-2 p-chip-info" />
                            <Chip label={`Kalan: ${izinOzeti.kalan} gün`} className="p-chip-success" />
                        </div>
                    </div>
                </Card>
            )}

            <Card>
                <Toolbar
                    className="p-mb-4"
                    left={leftToolbarTemplate}
                    right={rightToolbarTemplate}
                ></Toolbar>

                <DataTable
                    ref={dt}
                    value={izinTalepleri}
                    dataKey="id"
                    paginator
                    rows={10}
                    rowsPerPageOptions={[5, 10, 25]}
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                    currentPageReportTemplate="{first} - {last} arası, toplam {totalRecords} kayıt"
                    globalFilter={globalFilter}
                    header={header}
                    responsiveLayout="scroll"
                    loading={loading}
                    emptyMessage="İzin talebi bulunamadı."
                >
                    <Column
                        field="id"
                        header="ID"
                        sortable
                        style={{ minWidth: '4rem', width: '4rem' }}
                    ></Column>
                    <Column
                        field="personelAd"
                        header="Ad Soyad"
                        body={(rowData) => (
                            <div className="p-d-flex p-ai-center">
                                {avatarBodyTemplate(rowData)}
                                <span className="p-ml-2">{rowData.personelAd}</span>
                            </div>
                        )}
                        sortable
                        style={{ minWidth: '14rem' }}
                    ></Column>
                    <Column
                        field="personelDepartman"
                        header="Departman"
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="personelPozisyon"
                        header="Pozisyon"
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="izinTipi"
                        header="İzin Tipi"
                        body={izinTipiBodyTemplate}
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="izinBaslamaTarihi"
                        header="İzin Başlama"
                        body={tarihBodyTemplate('izinBaslamaTarihi')}
                        sortable
                        style={{ minWidth: '12rem' }}
                    ></Column>
                    <Column
                        field="isbasiTarihi"
                        header="İşbaşı"
                        body={tarihBodyTemplate('isbasiTarihi')}
                        sortable
                        style={{ minWidth: '12rem' }}
                    ></Column>
                    <Column
                        field="gunSayisi"
                        header="Gün"
                        sortable
                        style={{ minWidth: '6rem' }}
                    ></Column>
                    <Column
                        field="durum"
                        header="Durum"
                        body={durumBodyTemplate}
                        sortable
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        field="onaylayanAd"
                        header="Onaylayan"
                        sortable
                        style={{ minWidth: '12rem' }}
                    ></Column>
                    <Column
                        field="reddedenAd"
                        header="Reddeden"
                        sortable
                        style={{ minWidth: '12rem' }}
                    ></Column>
                    <Column
                        body={actionBodyTemplate}
                        header="İşlemler"
                        style={{ minWidth: '8rem' }}
                    ></Column>
                </DataTable>
            </Card>

            {/* İzin Talebi Dialog */}
            <Dialog
                visible={izinDialog}
                style={{ width: '600px' }}
                header="İzin Talebi Detayları"
                modal
                className="p-fluid"
                footer={izinDialogFooter}
                onHide={hideDialog}
            >
                <div className="p-field">
                    <label htmlFor="izinTipi">İzin Tipi *</label>
                    <Dropdown
                        id="izinTipi"
                        value={izinTalebi.izinTipi}
                        options={izinTipleri}
                        onChange={(e) => onInputChange(e, 'izinTipi')}
                        placeholder="İzin tipi seçiniz"
                    />
                </div>

                <div className="p-formgrid p-grid">
                    <div className="p-field p-col-6">
                        <label htmlFor="izinBaslamaTarihi">İzin Başlama Tarihi *</label>
                        <Calendar
                            id="izinBaslamaTarihi"
                            value={izinTalebi.izinBaslamaTarihi}
                            onChange={(e) => onDateChange(e, 'izinBaslamaTarihi')}
                            dateFormat="dd/mm/yy"
                            locale="tr"
                            placeholder="dd/mm/yyyy"
                            showIcon
                            minDate={new Date()}
                            className={submitted && !izinTalebi.izinBaslamaTarihi ? 'p-invalid' : ''}
                        />
                        {submitted && !izinTalebi.izinBaslamaTarihi && (
                            <small className="p-error">İzin başlama tarihi gereklidir.</small>
                        )}
                    </div>

                    <div className="p-field p-col-6">
                        <label htmlFor="izinBaslamaSaati">İzin Başlama Saati *</label>
                        <Dropdown
                            id="izinBaslamaSaati"
                            value={izinTalebi.izinBaslamaSaati}
                            options={[
                                {label: '08:00', value: '08:00'},
                                {label: '09:00', value: '09:00'},
                                {label: '10:00', value: '10:00'},
                                {label: '11:00', value: '11:00'},
                                {label: '12:00', value: '12:00'},
                                {label: '13:00', value: '13:00'},
                                {label: '14:00', value: '14:00'},
                                {label: '15:00', value: '15:00'},
                                {label: '16:00', value: '16:00'},
                                {label: '17:00', value: '17:00'},
                                {label: '18:00', value: '18:00'}
                            ]}
                            onChange={(e) => { onInputChange(e, 'izinBaslamaSaati'); setTimeout(() => calculateGunSayisi(), 100); }}
                            placeholder="Saat seçiniz"
                        />
                    </div>
                </div>

                <div className="p-formgrid p-grid">
                    <div className="p-field p-col-6">
                        <label htmlFor="isbasiTarihi">İşbaşı Tarihi *</label>
                        <Calendar
                            id="isbasiTarihi"
                            value={izinTalebi.isbasiTarihi}
                            onChange={(e) => onDateChange(e, 'isbasiTarihi')}
                            dateFormat="dd/mm/yy"
                            locale="tr"
                            placeholder="dd/mm/yyyy"
                            showIcon
                            minDate={izinTalebi.izinBaslamaTarihi || new Date()}
                            className={submitted && !izinTalebi.isbasiTarihi ? 'p-invalid' : ''}
                        />
                        {submitted && !izinTalebi.isbasiTarihi && (
                            <small className="p-error">İşbaşı tarihi gereklidir.</small>
                        )}
                    </div>

                    <div className="p-field p-col-6">
                        <label htmlFor="isbasiSaati">İşbaşı Saati *</label>
                        <Dropdown
                            id="isbasiSaati"
                            value={izinTalebi.isbasiSaati}
                            options={[
                                {label: '08:00', value: '08:00'},
                                {label: '09:00', value: '09:00'},
                                {label: '10:00', value: '10:00'},
                                {label: '11:00', value: '11:00'},
                                {label: '12:00', value: '12:00'},
                                {label: '13:00', value: '13:00'},
                                {label: '14:00', value: '14:00'},
                                {label: '15:00', value: '15:00'},
                                {label: '16:00', value: '16:00'},
                                {label: '17:00', value: '17:00'},
                                {label: '18:00', value: '18:00'}
                            ]}
                            onChange={(e) => { onInputChange(e, 'isbasiSaati'); setTimeout(() => calculateGunSayisi(), 100); }}
                            placeholder="Saat seçiniz"
                        />
                    </div>
                </div>

                {izinTalebi.gunSayisi > 0 && (
                    <div className="p-field">
                        <Message
                            severity="info"
                            text={`Hesaplanan Toplam İzin Sayısı: ${izinTalebi.gunSayisi} Gün${(izinTalebi.izinTipi === 'Ücretsiz İzin' || izinTalebi.izinTipi === 'Dış Görev') ? ' (Bakiyeden düşülmez)' : ''}`}
                        />
                    </div>
                )}

                {/* İzin tipi kuralları bilgilendirmesi */}
                {(() => {
                    const selectedIzinTipi = izinTipleri.find(tip => tip.value === izinTalebi.izinTipi);
                    if (!selectedIzinTipi) return null;

                    const rules = [];
                    if (selectedIzinTipi.minimumGunSayisi) {
                        rules.push(`Minimum ${selectedIzinTipi.minimumGunSayisi} gün`);
                    }
                    if (selectedIzinTipi.maksimumGunSayisi) {
                        rules.push(`Maksimum ${selectedIzinTipi.maksimumGunSayisi} gün`);
                    }
                    if (selectedIzinTipi.raporGerekli) {
                        rules.push('Rapor gereklidir');
                    }

                    if (rules.length > 0) {
                        return (
                            <div className="p-field">
                                <Message
                                    severity="warn"
                                    text={`${izinTalebi.izinTipi} Kuralları: ${rules.join(', ')}`}
                                />
                            </div>
                        );
                    }
                    return null;
                })()}

                {izinTalebi.izinTipi === 'Dış Görev' && (
                    <div className="p-field">
                        <label htmlFor="gorevYeri">Görev Yeri *</label>
                        <InputText
                            id="gorevYeri"
                            value={izinTalebi.gorevYeri}
                            onChange={(e) => onInputChange(e, 'gorevYeri')}
                            className={submitted && izinTalebi.izinTipi === 'Dış Görev' && !izinTalebi.gorevYeri ? 'p-invalid' : ''}
                        />
                        {submitted && izinTalebi.izinTipi === 'Dış Görev' && !izinTalebi.gorevYeri && (
                            <small className="p-error">Görev yeri gereklidir.</small>
                        )}
                    </div>
                )}

                {/* Rapor Yükleme (Şartlı) */}
                {(() => {
                    const selectedIzinTipi = izinTipleri.find(tip => tip.value === izinTalebi.izinTipi);
                    if (selectedIzinTipi && selectedIzinTipi.raporGerekli) {
                        return (
                            <div className="p-field">
                                <label htmlFor="rapor">Rapor Yükle *</label>
                                <FileUpload
                                    mode="basic"
                                    name="rapor"
                                    accept=".pdf,.doc,.docx,.jpg,.jpeg,.png"
                                    maxFileSize={10000000}
                                    chooseLabel={uploadedRapor ? uploadedRapor.name : "Rapor Seç"}
                                    className={submitted && !uploadedRapor ? 'p-invalid' : ''}
                                    onSelect={(e) => {
                                        if (e.files && e.files.length > 0) {
                                            setUploadedRapor(e.files[0]);
                                            toast.current.show({
                                                severity: 'success',
                                                summary: 'Başarılı',
                                                detail: 'Rapor seçildi: ' + e.files[0].name,
                                                life: 3000
                                            });
                                        }
                                    }}
                                />
                                {uploadedRapor && (
                                    <div className="p-mt-2">
                                        <small className="p-text-success">
                                            <i className="pi pi-check-circle mr-2"></i>
                                            Seçilen dosya: {uploadedRapor.name}
                                        </small>
                                    </div>
                                )}
                                {submitted && !uploadedRapor && (
                                    <small className="p-error">Rapor yüklemeniz gerekmektedir.</small>
                                )}
                            </div>
                        );
                    }
                    return null;
                })()}

                <div className="p-field">
                    <label htmlFor="aciklama">İzin Açıklama</label>
                    <InputTextarea
                        id="aciklama"
                        value={izinTalebi.aciklama}
                        onChange={(e) => onInputChange(e, 'aciklama')}
                        rows={3}
                        cols={20}
                    />
                </div>
            </Dialog>

            {/* Onay/Reddet Dialog */}
            <Dialog
                visible={onayDialog}
                style={{ width: '500px' }}
                header={`İzin Talebi ${onayTipi === 'onayla' ? 'Onaylama' : 'Reddetme'}`}
                modal
                className="p-fluid"
                footer={onayDialogFooter}
                onHide={hideOnayDialog}
            >
                {selectedIzin && (
                    <>
                        <Panel header="İzin Talebi Bilgileri" className="p-mb-3">
                            <div className="p-d-flex p-ai-center p-mb-2">
                                {avatarBodyTemplate(selectedIzin)}
                                <div className="p-ml-2">
                                    <div><strong>{selectedIzin.personelAd}</strong></div>
                                    <div className="p-text-secondary">{selectedIzin.personelDepartman}</div>
                                </div>
                            </div>
                            <div className="p-mb-2">
                                <strong>İzin Tipi:</strong> {selectedIzin.izinTipi}
                            </div>
                            <div className="p-mb-2">
                                <strong>Tarih:</strong> {izinService.formatTarih(selectedIzin.izinBaslamaTarihi || selectedIzin.baslangicTarihi)} - {izinService.formatTarih(selectedIzin.isbasiTarihi || selectedIzin.bitisTarihi)}
                            </div>
                            <div className="p-mb-2">
                                <strong>Gün Sayısı:</strong> {selectedIzin.gunSayisi} gün
                            </div>
                            {selectedIzin.aciklama && (
                                <div>
                                    <strong>Açıklama:</strong> {selectedIzin.aciklama}
                                </div>
                            )}
                        </Panel>

                        <div className="p-field">
                            <label htmlFor="onayNotu">
                                {onayTipi === 'onayla' ? 'Onay Notu' : 'Reddet Notu'}
                            </label>
                            <InputTextarea
                                id="onayNotu"
                                value={onayNotu}
                                onChange={(e) => setOnayNotu(e.target.value)}
                                rows={3}
                                placeholder={onayTipi === 'onayla' ? 'Onay nedeninizi yazabilirsiniz...' : 'Reddet nedeninizi yazın...'}
                            />
                        </div>
                    </>
                )}
            </Dialog>
        </div>
    );
};

export default IzinTalepleri;