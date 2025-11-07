import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Dropdown } from 'primereact/dropdown';
import { InputSwitch } from 'primereact/inputswitch';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { Tag } from 'primereact/tag';
import { confirmDialog } from 'primereact/confirmdialog';
import lucaBordroAyarlariService from '../services/lucaBordroAyarlariService';

const LucaAyarlari = () => {
    const [ayarlar, setAyarlar] = useState([]);
    const [loading, setLoading] = useState(false);
    const [dialogVisible, setDialogVisible] = useState(false);
    const [testLoading, setTestLoading] = useState(false);
    const [ayar, setAyar] = useState(null);
    const toast = useRef(null);

    const baglantiTipleri = [
        { label: 'API', value: 'API' },
        { label: 'Dosya', value: 'Dosya' },
        { label: 'Her İkisi', value: 'Ikisi' }
    ];

    const emptyAyar = {
        baglantiTipi: 'API',
        apiUrl: '',
        apiKey: '',
        apiUsername: '',
        apiPassword: '',
        dosyaYolu: '',
        otomatikSenkron: false,
        senkronSaati: '',
        aktif: true
    };

    useEffect(() => {
        loadAyarlar();
    }, []);

    const loadAyarlar = async () => {
        setLoading(true);
        try {
            const response = await lucaBordroAyarlariService.getAll();
            if (response.success) {
                setAyarlar(response.data || []);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Ayarlar yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const openNew = () => {
        setAyar(emptyAyar);
        setDialogVisible(true);
    };

    const hideDialog = () => {
        setDialogVisible(false);
        setAyar(null);
    };

    const saveAyar = async () => {
        setLoading(true);
        try {
            let response;
            if (ayar.id) {
                response = await lucaBordroAyarlariService.update(ayar.id, ayar);
            } else {
                response = await lucaBordroAyarlariService.create(ayar);
            }

            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message || 'Ayarlar kaydedildi',
                    life: 3000
                });
                hideDialog();
                loadAyarlar();
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Kayıt işlemi başarısız',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const editAyar = (ayar) => {
        setAyar({ ...ayar });
        setDialogVisible(true);
    };

    const confirmDelete = (ayar) => {
        confirmDialog({
            message: 'Bu ayarı silmek istediğinizden emin misiniz?',
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deleteAyar(ayar),
            acceptLabel: 'Evet',
            rejectLabel: 'Hayır',
            acceptClassName: 'p-button-danger'
        });
    };

    const deleteAyar = async (ayar) => {
        setLoading(true);
        try {
            const response = await lucaBordroAyarlariService.delete(ayar.id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Ayar silindi',
                    life: 3000
                });
                loadAyarlar();
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Silme işlemi başarısız',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const testBaglanti = async (ayar) => {
        setTestLoading(true);
        try {
            const response = await lucaBordroAyarlariService.testBaglanti(ayar.id);
            toast.current.show({
                severity: response.success ? 'success' : 'error',
                summary: response.success ? 'Başarılı' : 'Hata',
                detail: response.message,
                life: 3000
            });
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Bağlantı testi başarısız',
                life: 3000
            });
        } finally {
            setTestLoading(false);
        }
    };

    const onInputChange = (e, name) => {
        const val = (e.target && e.target.value) || '';
        let _ayar = { ...ayar };
        _ayar[`${name}`] = val;
        setAyar(_ayar);
    };

    const onSwitchChange = (e, name) => {
        let _ayar = { ...ayar };
        _ayar[`${name}`] = e.value;
        setAyar(_ayar);
    };

    const leftToolbarTemplate = () => {
        return (
            <Button
                label="Yeni Ayar"
                icon="pi pi-plus"
                className="p-button-success"
                onClick={openNew}
            />
        );
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-2">
                <Button
                    icon="pi pi-pencil"
                    className="p-button-rounded p-button-warning"
                    onClick={() => editAyar(rowData)}
                    tooltip="Düzenle"
                    tooltipOptions={{ position: 'top' }}
                />
                <Button
                    icon="pi pi-trash"
                    className="p-button-rounded p-button-danger"
                    onClick={() => confirmDelete(rowData)}
                    tooltip="Sil"
                    tooltipOptions={{ position: 'top' }}
                />
                <Button
                    icon="pi pi-bolt"
                    className="p-button-rounded p-button-info"
                    onClick={() => testBaglanti(rowData)}
                    loading={testLoading}
                    tooltip="Bağlantı Testi"
                    tooltipOptions={{ position: 'top' }}
                />
            </div>
        );
    };

    const aktifBodyTemplate = (rowData) => {
        return (
            <Tag
                value={rowData.aktif ? 'Aktif' : 'Pasif'}
                severity={rowData.aktif ? 'success' : 'warning'}
            />
        );
    };

    const baglantiTipiBodyTemplate = (rowData) => {
        return <span>{rowData.baglantiTipi}</span>;
    };

    const sonSenkronBodyTemplate = (rowData) => {
        if (!rowData.sonSenkronTarihi) return '-';
        return new Date(rowData.sonSenkronTarihi).toLocaleString('tr-TR');
    };

    const dialogFooter = (
        <div>
            <Button
                label="İptal"
                icon="pi pi-times"
                className="p-button-text"
                onClick={hideDialog}
            />
            <Button
                label="Kaydet"
                icon="pi pi-check"
                onClick={saveAyar}
                loading={loading}
            />
        </div>
    );

    return (
        <div className="grid">
            <div className="col-12">
                <div className="card">
                    <Toast ref={toast} />
                    <h5>Luca Bordro Ayarları</h5>

                    <Toolbar className="mb-4" left={leftToolbarTemplate}></Toolbar>

                    <DataTable
                        value={ayarlar}
                        loading={loading}
                        responsiveLayout="scroll"
                        emptyMessage="Ayar bulunamadı"
                    >
                        <Column field="baglantiTipi" header="Bağlantı Tipi" body={baglantiTipiBodyTemplate} />
                        <Column field="apiUrl" header="API URL" />
                        <Column field="aktif" header="Durum" body={aktifBodyTemplate} />
                        <Column field="otomatikSenkron" header="Otomatik Senkron" body={(data) => data.otomatikSenkron ? 'Evet' : 'Hayır'} />
                        <Column field="senkronSaati" header="Senkron Saati" />
                        <Column field="sonSenkronTarihi" header="Son Senkron" body={sonSenkronBodyTemplate} />
                        <Column body={actionBodyTemplate} exportable={false} style={{ minWidth: '12rem' }} />
                    </DataTable>

                    <Dialog
                        visible={dialogVisible}
                        style={{ width: '600px' }}
                        header="Luca Ayar Detayı"
                        modal
                        className="p-fluid"
                        footer={dialogFooter}
                        onHide={hideDialog}
                    >
                        <div className="field">
                            <label htmlFor="baglantiTipi">Bağlantı Tipi *</label>
                            <Dropdown
                                id="baglantiTipi"
                                value={ayar?.baglantiTipi}
                                onChange={(e) => onInputChange(e, 'baglantiTipi')}
                                options={baglantiTipleri}
                                placeholder="Bağlantı tipi seçin"
                            />
                        </div>

                        {(ayar?.baglantiTipi === 'API' || ayar?.baglantiTipi === 'Ikisi') && (
                            <>
                                <div className="field">
                                    <label htmlFor="apiUrl">API URL</label>
                                    <InputText
                                        id="apiUrl"
                                        value={ayar?.apiUrl}
                                        onChange={(e) => onInputChange(e, 'apiUrl')}
                                        placeholder="https://api.luca.com/..."
                                    />
                                </div>

                                <div className="field">
                                    <label htmlFor="apiKey">API Key</label>
                                    <InputText
                                        id="apiKey"
                                        value={ayar?.apiKey}
                                        onChange={(e) => onInputChange(e, 'apiKey')}
                                        type="password"
                                    />
                                </div>

                                <div className="field">
                                    <label htmlFor="apiUsername">API Kullanıcı Adı</label>
                                    <InputText
                                        id="apiUsername"
                                        value={ayar?.apiUsername}
                                        onChange={(e) => onInputChange(e, 'apiUsername')}
                                    />
                                </div>

                                <div className="field">
                                    <label htmlFor="apiPassword">API Şifre</label>
                                    <InputText
                                        id="apiPassword"
                                        value={ayar?.apiPassword}
                                        onChange={(e) => onInputChange(e, 'apiPassword')}
                                        type="password"
                                    />
                                </div>
                            </>
                        )}

                        {(ayar?.baglantiTipi === 'Dosya' || ayar?.baglantiTipi === 'Ikisi') && (
                            <div className="field">
                                <label htmlFor="dosyaYolu">Dosya Yolu</label>
                                <InputText
                                    id="dosyaYolu"
                                    value={ayar?.dosyaYolu}
                                    onChange={(e) => onInputChange(e, 'dosyaYolu')}
                                    placeholder="C:\Luca\Bordrolar"
                                />
                            </div>
                        )}

                        <div className="field-checkbox">
                            <InputSwitch
                                id="otomatikSenkron"
                                checked={ayar?.otomatikSenkron}
                                onChange={(e) => onSwitchChange(e, 'otomatikSenkron')}
                            />
                            <label htmlFor="otomatikSenkron" className="ml-2">Otomatik Senkronizasyon</label>
                        </div>

                        {ayar?.otomatikSenkron && (
                            <div className="field">
                                <label htmlFor="senkronSaati">Senkron Saati (HH:mm)</label>
                                <InputText
                                    id="senkronSaati"
                                    value={ayar?.senkronSaati}
                                    onChange={(e) => onInputChange(e, 'senkronSaati')}
                                    placeholder="09:00"
                                />
                            </div>
                        )}

                        <div className="field-checkbox">
                            <InputSwitch
                                id="aktif"
                                checked={ayar?.aktif}
                                onChange={(e) => onSwitchChange(e, 'aktif')}
                            />
                            <label htmlFor="aktif" className="ml-2">Aktif</label>
                        </div>
                    </Dialog>
                </div>
            </div>
        </div>
    );
};

export default LucaAyarlari;
