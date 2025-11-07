import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { InputTextarea } from 'primereact/inputtextarea';
import { confirmDialog } from 'primereact/confirmdialog';
import { Card } from 'primereact/card';
import { InputSwitch } from 'primereact/inputswitch';
import { Tag } from 'primereact/tag';
import kvkkService from '../services/kvkkService';
import authService from '../services/authService';
import yetkiService from '../services/yetkiService';

const KVKKIzinMetni = () => {
    const [metinler, setMetinler] = useState([]);
    const [metinDialog, setMetinDialog] = useState(false);
    const [metin, setMetin] = useState({
        id: null,
        baslik: '',
        metin: '',
        aktif: true,
        versiyon: 1
    });
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
    const [saving, setSaving] = useState(false);
    const [permissions, setPermissions] = useState({
        read: false,
        write: false,
        delete: false,
        update: false
    });
    const toast = useRef(null);
    const dt = useRef(null);

    useEffect(() => {
        loadMetinler();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('kvkk-izin-metni', 'read'),
                write: yetkiService.hasScreenPermission('kvkk-izin-metni', 'write'),
                delete: yetkiService.hasScreenPermission('kvkk-izin-metni', 'delete'),
                update: yetkiService.hasScreenPermission('kvkk-izin-metni', 'update')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
            setPermissions({
                read: false,
                write: false,
                delete: false,
                update: false
            });
        }
    };

    const loadMetinler = async () => {
        setLoading(true);
        try {
            const response = await kvkkService.getAll();
            if (response.success) {
                setMetinler(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'KVKK metinleri yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const openNew = () => {
        const user = authService.getUser();
        const maxVersiyon = metinler.length > 0 ? Math.max(...metinler.map(m => m.versiyon)) : 0;

        setMetin({
            id: null,
            baslik: '',
            metin: '',
            aktif: false,
            versiyon: maxVersiyon + 1,
            olusturanPersonelId: user?.personel?.id
        });
        setSubmitted(false);
        setMetinDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setMetinDialog(false);
    };

    const saveMetin = async () => {
        setSubmitted(true);

        if (metin.baslik.trim() && metin.metin.trim() && !saving) {
            setSaving(true);
            try {
                const dataToSend = { ...metin };
                if (!dataToSend.id) {
                    delete dataToSend.id;
                }
                // Versiyon'u sayıya çevir
                if (dataToSend.versiyon) {
                    dataToSend.versiyon = parseInt(dataToSend.versiyon, 10);
                }

                let response;
                if (metin.id) {
                    response = await kvkkService.update(metin.id, dataToSend);
                } else {
                    response = await kvkkService.create(dataToSend);
                }

                if (response.success) {
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: response.message,
                        life: 3000
                    });
                    loadMetinler();
                    setMetinDialog(false);
                    setMetin({
                        id: null,
                        baslik: '',
                        metin: '',
                        aktif: false,
                        versiyon: 1
                    });
                }
            } catch (error) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: error.message,
                    life: 3000
                });
            } finally {
                setSaving(false);
            }
        }
    };

    const editMetin = (metin) => {
        setMetin({ ...metin });
        setMetinDialog(true);
    };

    const confirmDeleteMetin = (metin) => {
        confirmDialog({
            message: `${metin.baslik} başlıklı KVKK metnini silmek istediğinizden emin misiniz?`,
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'Evet',
            rejectLabel: 'Hayır',
            accept: () => deleteMetin(metin)
        });
    };

    const deleteMetin = async (metin) => {
        try {
            const response = await kvkkService.delete(metin.id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadMetinler();
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

    const onInputChange = (e, name) => {
        const val = (e.target && e.target.value) || '';
        let _metin = { ...metin };
        _metin[name] = val;
        setMetin(_metin);
    };


    const onSwitchChange = (e, name) => {
        let _metin = { ...metin };
        _metin[name] = e.value;
        setMetin(_metin);
    };

    const leftToolbarTemplate = () => {
        return (
            <div className="flex flex-wrap gap-2">
                {permissions.write && (
                    <Button
                        label="Yeni KVKK Metni"
                        icon="pi pi-plus"
                        severity="success"
                        onClick={openNew}
                    />
                )}
            </div>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <span className="p-input-icon-left">
                <i className="pi pi-search" />
                <InputText
                    type="search"
                    onInput={(e) => setGlobalFilter(e.target.value)}
                    placeholder="Ara..."
                />
            </span>
        );
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-2">
                {permissions.update && (
                    <Button
                        icon="pi pi-pencil"
                        rounded
                        outlined
                        className="mr-2"
                        onClick={() => editMetin(rowData)}
                    />
                )}
                {permissions.delete && (
                    <Button
                        icon="pi pi-trash"
                        rounded
                        outlined
                        severity="danger"
                        onClick={() => confirmDeleteMetin(rowData)}
                    />
                )}
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

    const versiyonBodyTemplate = (rowData) => {
        return <Tag value={`v${rowData.versiyon}`} severity="info" />;
    };

    const tarihBodyTemplate = (rowData) => {
        if (!rowData.createdAt) return '-';
        return new Date(rowData.createdAt).toLocaleDateString('tr-TR');
    };

    const olusturanBodyTemplate = (rowData) => {
        return rowData.olusturanPersonel?.adSoyad || '-';
    };

    const metinDialogFooter = (
        <>
            <Button
                label="İptal"
                icon="pi pi-times"
                outlined
                onClick={hideDialog}
                disabled={saving}
            />
            <Button
                label="Kaydet"
                icon="pi pi-check"
                onClick={saveMetin}
                loading={saving}
            />
        </>
    );

    const header = (
        <div className="flex flex-wrap gap-2 align-items-center justify-content-between">
            <h4 className="m-0">KVKK İzin Metinleri</h4>
            <span className="p-input-icon-left">
                <i className="pi pi-search" />
                <InputText
                    type="search"
                    onInput={(e) => setGlobalFilter(e.target.value)}
                    placeholder="Ara..."
                />
            </span>
        </div>
    );

    if (!permissions.read) {
        return (
            <div className="card">
                <h5>KVKK İzin Metinleri</h5>
                <p>Bu sayfayı görüntüleme yetkiniz bulunmamaktadır.</p>
            </div>
        );
    }

    return (
        <div className="grid">
            <div className="col-12">
                <Card>
                    <Toast ref={toast} />
                    <Toolbar className="mb-4" left={leftToolbarTemplate} right={rightToolbarTemplate} />

                    <DataTable
                        ref={dt}
                        value={metinler}
                        dataKey="id"
                        paginator
                        rows={10}
                        rowsPerPageOptions={[5, 10, 25]}
                        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                        currentPageReportTemplate="{first} - {last} / {totalRecords} kayıt"
                        globalFilter={globalFilter}
                        header={header}
                        loading={loading}
                        emptyMessage="KVKK metni bulunamadı."
                    >
                        <Column
                            field="versiyon"
                            header="Versiyon"
                            body={versiyonBodyTemplate}
                            sortable
                            style={{ minWidth: '8rem' }}
                        />
                        <Column
                            field="baslik"
                            header="Başlık"
                            sortable
                            style={{ minWidth: '15rem' }}
                        />
                        <Column
                            field="aktif"
                            header="Durum"
                            body={aktifBodyTemplate}
                            sortable
                            style={{ minWidth: '8rem' }}
                        />
                        <Column
                            field="createdAt"
                            header="Oluşturulma Tarihi"
                            body={tarihBodyTemplate}
                            sortable
                            style={{ minWidth: '12rem' }}
                        />
                        <Column
                            field="olusturanPersonel"
                            header="Oluşturan"
                            body={olusturanBodyTemplate}
                            sortable
                            style={{ minWidth: '12rem' }}
                        />
                        <Column
                            body={actionBodyTemplate}
                            exportable={false}
                            style={{ minWidth: '8rem' }}
                        />
                    </DataTable>

                    <Dialog
                        visible={metinDialog}
                        style={{ width: '70vw' }}
                        breakpoints={{ '960px': '75vw', '641px': '90vw' }}
                        header="KVKK İzin Metni Detayları"
                        modal
                        className="p-fluid"
                        footer={metinDialogFooter}
                        onHide={hideDialog}
                    >
                        <div className="field">
                            <label htmlFor="baslik">Başlık *</label>
                            <InputText
                                id="baslik"
                                value={metin.baslik}
                                onChange={(e) => onInputChange(e, 'baslik')}
                                required
                                autoFocus
                                className={submitted && !metin.baslik ? 'p-invalid' : ''}
                            />
                            {submitted && !metin.baslik && (
                                <small className="p-error">Başlık gereklidir.</small>
                            )}
                        </div>

                        <div className="field">
                            <label htmlFor="versiyon">Versiyon</label>
                            <InputText
                                id="versiyon"
                                value={metin.versiyon}
                                onChange={(e) => onInputChange(e, 'versiyon')}
                                keyfilter="pint"
                            />
                        </div>

                        <div className="field">
                            <label htmlFor="metin">KVKK Metni *</label>
                            <InputTextarea
                                id="metin"
                                value={metin.metin}
                                onChange={(e) => onInputChange(e, 'metin')}
                                rows={15}
                                style={{ width: '100%' }}
                                className={submitted && !metin.metin ? 'p-invalid' : ''}
                            />
                            {submitted && !metin.metin && (
                                <small className="p-error">Metin gereklidir.</small>
                            )}
                        </div>

                        <div className="field">
                            <label htmlFor="aktif">Aktif</label>
                            <div>
                                <InputSwitch
                                    id="aktif"
                                    checked={metin.aktif}
                                    onChange={(e) => onSwitchChange(e, 'aktif')}
                                />
                                <span className="ml-2">
                                    {metin.aktif ? 'Bu metin aktif olarak yayınlanacaktır' : 'Bu metin pasif durumda'}
                                </span>
                            </div>
                            {metin.aktif && (
                                <small className="p-info block mt-2">
                                    Not: Aktif olarak işaretlediğinizde, diğer tüm KVKK metinleri otomatik olarak pasif hale gelecektir.
                                </small>
                            )}
                        </div>
                    </Dialog>
                </Card>
            </div>
        </div>
    );
};

export default KVKKIzinMetni;
