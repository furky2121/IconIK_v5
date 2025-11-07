import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { InputNumber } from 'primereact/inputnumber';
import { Dropdown } from 'primereact/dropdown';
import { Checkbox } from 'primereact/checkbox';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { Card } from 'primereact/card';
import { TabView, TabPanel } from 'primereact/tabview';
import { Badge } from 'primereact/badge';
import { Message } from 'primereact/message';
import { Divider } from 'primereact/divider';
import { Panel } from 'primereact/panel';
import { confirmDialog } from 'primereact/confirmdialog';
import yetkiService from '../services/yetkiService';
import izinKonfigurasyonService from '../services/izinKonfigurasyonService';

const IzinKonfigurasyonlari = () => {
    const [izinTipleri, setIzinTipleri] = useState([]);
    const [yillikIzinKurallari, setYillikIzinKurallari] = useState([]);
    const [loading, setLoading] = useState(false);
    const [izinTipiDialog, setIzinTipiDialog] = useState(false);
    const [yillikIzinDialog, setYillikIzinDialog] = useState(false);
    const [selectedIzinTipi, setSelectedIzinTipi] = useState(null);
    const [selectedYillikKural, setSelectedYillikKural] = useState(null);
    const [submitted, setSubmitted] = useState(false);
    const [activeIndex, setActiveIndex] = useState(0);
    const [globalFilter, setGlobalFilter] = useState(null);

    // Permission states
    const [permissions, setPermissions] = useState({
        read: false,
        write: false,
        delete: false,
        update: false
    });

    const toast = useRef(null);
    const dt = useRef(null);

    const emptyIzinTipi = {
        id: null,
        izinTipiAdi: '',
        standartGunSayisi: null,
        minimumGunSayisi: null,
        maksimumGunSayisi: null,
        cinsiyetKisiti: null, // null: Herkese, 'Kadın': Kadın, 'Erkek': Erkek
        raporGerekli: false,
        ucretliMi: true,
        renk: null,
        aciklama: '',
        sira: 0,
        aktif: true
    };

    const emptyYillikKural = {
        id: null,
        minKidemYili: 0,
        maxKidemYili: null,
        izinGunSayisi: 14,
        aciklama: '',
        aktif: true
    };

    const cinsiyetSecenekleri = [
        { label: 'Herkese', value: null },
        { label: 'Sadece Kadınlar', value: 'Kadın' },
        { label: 'Sadece Erkekler', value: 'Erkek' }
    ];

    useEffect(() => {
        loadData();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('izin-konfigurasyonlari', 'read'),
                write: yetkiService.hasScreenPermission('izin-konfigurasyonlari', 'write'),
                delete: yetkiService.hasScreenPermission('izin-konfigurasyonlari', 'delete'),
                update: yetkiService.hasScreenPermission('izin-konfigurasyonlari', 'update')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
            setPermissions({
                read: true, // Geçici olarak true, servis hazır olana kadar
                write: true,
                delete: true,
                update: true
            });
        }
    };

    const loadData = async () => {
        setLoading(true);
        try {
            // Load İzin Tipleri from API
            const izinTipleriResponse = await izinKonfigurasyonService.getAllIzinTipleri();
            if (izinTipleriResponse.success) {
                setIzinTipleri(izinTipleriResponse.data);
            }

            // Load Yıllık İzin Kuralları from API (if API is ready)
            // For now, using empty array until Yıllık İzin Kuralları API is implemented
            setYillikIzinKurallari([]);

        } catch (error) {
            console.error('Data loading error:', error);
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Veriler yüklenirken hata oluştu: ' + (error.message || 'Bilinmeyen hata')
            });
        } finally {
            setLoading(false);
        }
    };

    const openNewIzinTipi = () => {
        setSelectedIzinTipi({ ...emptyIzinTipi });
        setSubmitted(false);
        setIzinTipiDialog(true);
    };

    const hideIzinTipiDialog = () => {
        setSubmitted(false);
        setIzinTipiDialog(false);
    };

    const saveIzinTipi = async () => {
        setSubmitted(true);

        if (selectedIzinTipi.izinTipiAdi.trim()) {
            try {
                let response;
                if (selectedIzinTipi.id) {
                    // Update existing
                    response = await izinKonfigurasyonService.updateIzinTipi(selectedIzinTipi.id, selectedIzinTipi);
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: 'İzin tipi güncellendi'
                    });
                } else {
                    // Create new
                    response = await izinKonfigurasyonService.createIzinTipi(selectedIzinTipi);
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: 'İzin tipi eklendi'
                    });
                }

                if (response.success) {
                    loadData(); // Reload data from API
                    setIzinTipiDialog(false);
                    setSelectedIzinTipi(emptyIzinTipi);
                }
            } catch (error) {
                console.error('Save error:', error);
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: 'İzin tipi kaydedilirken hata oluştu: ' + (error.message || 'Bilinmeyen hata')
                });
            }
        }
    };

    const editIzinTipi = (izinTipi) => {
        setSelectedIzinTipi({ ...izinTipi });
        setIzinTipiDialog(true);
    };

    const confirmDeleteIzinTipi = (izinTipi) => {
        confirmDialog({
            message: `"${izinTipi.izinTipiAdi}" adlı izin tipini silmek istediğinizden emin misiniz?`,
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deleteIzinTipi(izinTipi)
        });
    };

    const deleteIzinTipi = async (izinTipi) => {
        try {
            const response = await izinKonfigurasyonService.deleteIzinTipi(izinTipi.id);
            if (response.success) {
                loadData(); // Reload data from API
                setSelectedIzinTipi(emptyIzinTipi);
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'İzin tipi silindi'
                });
            }
        } catch (error) {
            console.error('Delete error:', error);
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'İzin tipi silinirken hata oluştu: ' + (error.message || 'Bilinmeyen hata')
            });
        }
    };

    const findIndexById = (id, array) => {
        let index = -1;
        for (let i = 0; i < array.length; i++) {
            if (array[i].id === id) {
                index = i;
                break;
            }
        }
        return index;
    };

    const createId = () => {
        let id = '';
        let chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        for (let i = 0; i < 5; i++) {
            id += chars.charAt(Math.floor(Math.random() * chars.length));
        }
        return id;
    };

    const onInputChange = (e, name) => {
        const val = (e.target && e.target.value) || '';
        let _izinTipi = { ...selectedIzinTipi };
        _izinTipi[`${name}`] = val;
        setSelectedIzinTipi(_izinTipi);
    };

    const onInputNumberChange = (e, name) => {
        const val = e.value || 0;
        let _izinTipi = { ...selectedIzinTipi };
        _izinTipi[`${name}`] = val;
        setSelectedIzinTipi(_izinTipi);
    };

    const onCheckboxChange = (e, name) => {
        let _izinTipi = { ...selectedIzinTipi };
        _izinTipi[`${name}`] = e.checked;
        setSelectedIzinTipi(_izinTipi);
    };

    const leftToolbarTemplate = () => {
        return (
            <div className="flex flex-wrap gap-2">
                <Button
                    label="Yeni İzin Tipi"
                    icon="pi pi-plus"
                    severity="success"
                    onClick={openNewIzinTipi}
                    disabled={!permissions.write}
                />
            </div>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <React.Fragment>
                <div className="flex align-items-center gap-2">
                    <span className="p-input-icon-left">
                        <i className="pi pi-search" />
                        <InputText
                            type="search"
                            onInput={(e) => setGlobalFilter(e.target.value)}
                            placeholder="Ara..."
                            className="w-20rem"
                        />
                    </span>
                </div>
            </React.Fragment>
        );
    };

    const aktifBodyTemplate = (rowData) => {
        return (
            <Badge
                value={rowData.aktif ? 'Aktif' : 'Pasif'}
                severity={rowData.aktif ? 'success' : 'danger'}
            />
        );
    };

    const cinsiyetBodyTemplate = (rowData) => {
        if (rowData.cinsiyetKisiti === 'K') {
            return <Badge value="Sadece Kadınlar" severity="warning" />;
        } else if (rowData.cinsiyetKisiti === 'E') {
            return <Badge value="Sadece Erkekler" severity="info" />;
        } else {
            return <Badge value="Herkese" severity="success" />;
        }
    };

    const raporBodyTemplate = (rowData) => {
        return rowData.raporGerekli ? (
            <i className="pi pi-check-circle" style={{ color: 'green' }}></i>
        ) : (
            <i className="pi pi-times-circle" style={{ color: 'red' }}></i>
        );
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <React.Fragment>
                <Button
                    icon="pi pi-pencil"
                    rounded
                    outlined
                    className="mr-2"
                    onClick={() => editIzinTipi(rowData)}
                    disabled={!permissions.update}
                />
                <Button
                    icon="pi pi-trash"
                    rounded
                    outlined
                    severity="danger"
                    onClick={() => confirmDeleteIzinTipi(rowData)}
                    disabled={!permissions.delete}
                />
            </React.Fragment>
        );
    };

    const header = (
        <div className="flex flex-wrap gap-2 align-items-center justify-content-between">
            <h4 className="m-0">İzin Tipleri</h4>
        </div>
    );

    const izinTipiDialogFooter = (
        <React.Fragment>
            <Button
                label="İptal"
                icon="pi pi-times"
                outlined
                onClick={hideIzinTipiDialog}
            />
            <Button
                label="Kaydet"
                icon="pi pi-check"
                onClick={saveIzinTipi}
            />
        </React.Fragment>
    );

    if (!permissions.read) {
        return (
            <div className="card">
                <Message severity="warn" text="Bu sayfayı görüntüleme yetkiniz bulunmamaktadır." />
            </div>
        );
    }

    return (
        <div className="grid">
            <Toast ref={toast} />

            <div className="col-12">
                <Card title="İzin Konfigürasyonları" className="shadow-3">
                    <TabView activeIndex={activeIndex} onTabChange={(e) => setActiveIndex(e.index)}>

                        {/* İzin Tipleri Yönetimi */}
                        <TabPanel header="İzin Tipleri" leftIcon="pi pi-list">
                            <div className="card">
                                <Toolbar className="mb-4" left={leftToolbarTemplate} right={rightToolbarTemplate} />

                                <DataTable
                                    ref={dt}
                                    value={izinTipleri}
                                    dataKey="id"
                                    paginator
                                    rows={10}
                                    rowsPerPageOptions={[5, 10, 25]}
                                    className="datatable-responsive"
                                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                                    currentPageReportTemplate="{first} - {last} arası, toplam {totalRecords} kayıt"
                                    globalFilter={globalFilter}
                                    emptyMessage="İzin tipi bulunamadı."
                                    header={header}
                                    loading={loading}
                                >
                                    <Column field="izinTipiAdi" header="İzin Tipi" sortable />
                                    <Column field="minimumGunSayisi" header="Minimum Gün" sortable />
                                    <Column field="maksimumGunSayisi" header="Maksimum Gün" sortable />
                                    <Column field="cinsiyetKisiti" header="Cinsiyet Kısıtı" body={cinsiyetBodyTemplate} />
                                    <Column field="raporGerekli" header="Rapor Gerekli" body={raporBodyTemplate} />
                                    <Column field="aktif" header="Durum" body={aktifBodyTemplate} />
                                    <Column body={actionBodyTemplate} exportable={false} style={{ minWidth: '8rem' }} />
                                </DataTable>
                            </div>
                        </TabPanel>

                        {/* Yıllık İzin Kuralları */}
                        <TabPanel header="Yıllık İzin Kuralları" leftIcon="pi pi-calendar">
                            <div className="grid">
                                <div className="col-12">
                                    <Panel header="Türkiye Yasal Düzenlemeler" className="mb-4">
                                        <div className="grid">
                                            <div className="col-12 md:col-4">
                                                <div className="surface-card p-4 shadow-2 border-round text-center">
                                                    <div className="text-900 font-medium text-xl mb-2">1-5 Yıl</div>
                                                    <div className="text-blue-600 text-4xl font-bold mb-3">14 Gün</div>
                                                    <div className="text-500">İlk 5 yıllık dönem</div>
                                                </div>
                                            </div>
                                            <div className="col-12 md:col-4">
                                                <div className="surface-card p-4 shadow-2 border-round text-center">
                                                    <div className="text-900 font-medium text-xl mb-2">5-15 Yıl</div>
                                                    <div className="text-green-600 text-4xl font-bold mb-3">20 Gün</div>
                                                    <div className="text-500">Orta kademe deneyim</div>
                                                </div>
                                            </div>
                                            <div className="col-12 md:col-4">
                                                <div className="surface-card p-4 shadow-2 border-round text-center">
                                                    <div className="text-900 font-medium text-xl mb-2">15+ Yıl</div>
                                                    <div className="text-orange-600 text-4xl font-bold mb-3">26 Gün</div>
                                                    <div className="text-500">Üst düzey kıdem</div>
                                                </div>
                                            </div>
                                        </div>
                                        <Divider />
                                        <Message
                                            severity="info"
                                            text="Bu kurallar Türkiye İş Kanunu'na göre belirlenmiştir. Kıdem yılı hesaplaması işe giriş tarihi baz alınarak yapılır."
                                            className="w-full"
                                        />
                                    </Panel>
                                </div>
                            </div>
                        </TabPanel>

                        {/* Özel İzin Ayarları */}
                        <TabPanel header="Özel İzin Ayarları" leftIcon="pi pi-cog">
                            <div className="grid">
                                <div className="col-12 md:col-6">
                                    <Panel header="Doğum İzni Ayarları" className="mb-4">
                                        <div className="field">
                                            <label htmlFor="dogumIzniSuresi" className="font-medium">İzin Süresi (Gün)</label>
                                            <InputNumber
                                                id="dogumIzniSuresi"
                                                value={112}
                                                disabled
                                                suffix=" gün"
                                                className="w-full"
                                            />
                                            <small className="text-500">16 hafta (112 gün) yasal zorunluluk</small>
                                        </div>
                                        <div className="field-checkbox">
                                            <Checkbox inputId="sadaceKadinlar" checked={true} disabled />
                                            <label htmlFor="sadaceKadinlar" className="ml-2">Sadece kadın personel</label>
                                        </div>
                                    </Panel>
                                </div>

                                <div className="col-12 md:col-6">
                                    <Panel header="Mazeret İzni Ayarları" className="mb-4">
                                        <div className="field">
                                            <label htmlFor="mazeretMaksimum" className="font-medium">Yıllık Maksimum (Gün)</label>
                                            <InputNumber
                                                id="mazeretMaksimum"
                                                value={6}
                                                min={1}
                                                max={30}
                                                className="w-full"
                                            />
                                        </div>
                                        <div className="field-checkbox">
                                            <Checkbox inputId="mazeretBelge" checked={true} />
                                            <label htmlFor="mazeretBelge" className="ml-2">Belge zorunluluğu</label>
                                        </div>
                                    </Panel>
                                </div>

                                <div className="col-12 md:col-6">
                                    <Panel header="Evlilik İzni Ayarları" className="mb-4">
                                        <div className="field">
                                            <label htmlFor="evlilikSuresi" className="font-medium">İzin Süresi (Gün)</label>
                                            <InputNumber
                                                id="evlilikSuresi"
                                                value={3}
                                                min={1}
                                                max={7}
                                                className="w-full"
                                            />
                                        </div>
                                        <small className="text-500">Evlilik belgesi ile kullanılabilir</small>
                                    </Panel>
                                </div>

                                <div className="col-12 md:col-6">
                                    <Panel header="Ölüm İzni Ayarları" className="mb-4">
                                        <div className="field">
                                            <label htmlFor="olumYakin" className="font-medium">Yakın Akraba (Gün)</label>
                                            <InputNumber
                                                id="olumYakin"
                                                value={7}
                                                min={3}
                                                max={10}
                                                className="w-full"
                                            />
                                        </div>
                                        <div className="field">
                                            <label htmlFor="olumUzak" className="font-medium">Uzak Akraba (Gün)</label>
                                            <InputNumber
                                                id="olumUzak"
                                                value={3}
                                                min={1}
                                                max={5}
                                                className="w-full"
                                            />
                                        </div>
                                    </Panel>
                                </div>
                            </div>
                        </TabPanel>
                    </TabView>
                </Card>
            </div>

            {/* İzin Tipi Dialog */}
            <Dialog
                visible={izinTipiDialog}
                style={{ width: '450px' }}
                header="İzin Tipi Detayları"
                modal
                className="p-fluid"
                footer={izinTipiDialogFooter}
                onHide={hideIzinTipiDialog}
            >
                <div className="field">
                    <label htmlFor="izinTipiAdi">İzin Tipi Adı</label>
                    <InputText
                        id="izinTipiAdi"
                        value={selectedIzinTipi?.izinTipiAdi}
                        onChange={(e) => onInputChange(e, 'izinTipiAdi')}
                        required
                        autoFocus
                        className={submitted && !selectedIzinTipi?.izinTipiAdi ? 'p-invalid' : ''}
                    />
                    {submitted && !selectedIzinTipi?.izinTipiAdi && (
                        <small className="p-error">İzin tipi adı gereklidir.</small>
                    )}
                </div>

                <div className="field">
                    <label htmlFor="minimumGunSayisi">Minimum Gün Sayısı (Zorunlu Değil)</label>
                    <InputNumber
                        id="minimumGunSayisi"
                        value={selectedIzinTipi?.minimumGunSayisi}
                        onValueChange={(e) => onInputNumberChange(e, 'minimumGunSayisi')}
                        min={0}
                        placeholder="Boş bırakılabilir"
                    />
                    <small className="p-text-secondary">Örn: Yıllık İzin için boş bırakın (personel 1 gün de kullanabilsin)</small>
                </div>

                <div className="field">
                    <label htmlFor="maksimumGunSayisi">Maksimum Gün Sayısı (Zorunlu Değil)</label>
                    <InputNumber
                        id="maksimumGunSayisi"
                        value={selectedIzinTipi?.maksimumGunSayisi}
                        onValueChange={(e) => onInputNumberChange(e, 'maksimumGunSayisi')}
                        min={0}
                        placeholder="Boş bırakılabilir"
                    />
                    <small className="p-text-secondary">Not: Yıllık İzin için otomatik hesaplanır (kıdeme göre)</small>
                </div>

                <div className="field">
                    <label htmlFor="cinsiyetKisiti">Cinsiyet Kısıtı</label>
                    <Dropdown
                        id="cinsiyetKisiti"
                        value={selectedIzinTipi?.cinsiyetKisiti}
                        options={cinsiyetSecenekleri}
                        onChange={(e) => onInputChange(e, 'cinsiyetKisiti')}
                        placeholder="Cinsiyet kısıtı seçin"
                    />
                </div>

                <div className="field">
                    <label htmlFor="aciklama">Açıklama</label>
                    <InputText
                        id="aciklama"
                        value={selectedIzinTipi?.aciklama}
                        onChange={(e) => onInputChange(e, 'aciklama')}
                    />
                </div>

                <div className="field-checkbox">
                    <Checkbox
                        inputId="raporGerekli"
                        checked={selectedIzinTipi?.raporGerekli}
                        onChange={(e) => onCheckboxChange(e, 'raporGerekli')}
                    />
                    <label htmlFor="raporGerekli">Rapor Gerekli</label>
                </div>

                <div className="field-checkbox">
                    <Checkbox
                        inputId="aktif"
                        checked={selectedIzinTipi?.aktif}
                        onChange={(e) => onCheckboxChange(e, 'aktif')}
                    />
                    <label htmlFor="aktif">Aktif</label>
                </div>
            </Dialog>
        </div>
    );
};

export default IzinKonfigurasyonlari;