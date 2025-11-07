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
import { Dropdown } from 'primereact/dropdown';
import odemeTanimiService from '../services/odemeTanimiService';
import yetkiService from '../services/yetkiService';

const OdemeTanimlari = () => {
    const emptyOdeme = {
        id: null,
        kod: '',
        ad: '',
        odemeTuru: 'Sabit',
        aciklama: '',
        aktif: true
    };

    const [odemeler, setOdemeler] = useState([]);
    const [odemeDialog, setOdemeDialog] = useState(false);
    const [odeme, setOdeme] = useState(emptyOdeme);
    const [selectedOdemeler, setSelectedOdemeler] = useState(null);
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

    const odemeTuruOptions = [
        { label: 'Sabit', value: 'Sabit' },
        { label: 'Yüzdesel', value: 'Yuzdesel' },
        { label: 'Prim', value: 'Prim' },
        { label: 'İkramiye', value: 'Ikramiye' },
        { label: 'Fazla Mesai', value: 'FazlaMesai' },
        { label: 'Diğer', value: 'Diger' }
    ];

    useEffect(() => {
        loadOdemeler();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('odeme-tanimlari', 'read'),
                write: yetkiService.hasScreenPermission('odeme-tanimlari', 'write'),
                delete: yetkiService.hasScreenPermission('odeme-tanimlari', 'delete'),
                update: yetkiService.hasScreenPermission('odeme-tanimlari', 'update')
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

    const loadOdemeler = async () => {
        setLoading(true);
        try {
            const response = await odemeTanimiService.getAll();
            if (response.success) {
                setOdemeler(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Ödeme tanımları yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const openNew = () => {
        setOdeme(emptyOdeme);
        setSubmitted(false);
        setOdemeDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setOdemeDialog(false);
    };

    const saveOdeme = async () => {
        setSubmitted(true);

        if (odeme.kod.trim() && odeme.ad.trim() && !saving) {
            setSaving(true);
            try {
                const dataToSend = { ...odeme };
                if (!dataToSend.id) {
                    delete dataToSend.id;
                }

                let response;
                if (odeme.id) {
                    response = await odemeTanimiService.update(odeme.id, dataToSend);
                } else {
                    response = await odemeTanimiService.create(dataToSend);
                }

                if (response.success) {
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: response.message,
                        life: 3000
                    });
                    loadOdemeler();
                    setOdemeDialog(false);
                    setOdeme(emptyOdeme);
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

    const editOdeme = (odeme) => {
        setOdeme({ ...odeme });
        setOdemeDialog(true);
    };

    const confirmDeleteOdeme = (odeme) => {
        confirmDialog({
            message: `${odeme.ad} ödeme tanımını silmek istediğinizden emin misiniz?`,
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deleteOdeme(odeme.id)
        });
    };

    const deleteOdeme = async (id) => {
        try {
            const response = await odemeTanimiService.delete(id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadOdemeler();
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

    const exportCSV = () => {
        dt.current.exportCSV();
    };

    const onInputChange = (e, name) => {
        const val = (e.target && e.target.value) || '';
        let _odeme = { ...odeme };
        _odeme[`${name}`] = val;
        setOdeme(_odeme);
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                {permissions.write && (
                    <Button
                        label="Yeni Ödeme Tanımı"
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
                    onClick={exportCSV}
                />
            </React.Fragment>
        );
    };

    const statusBodyTemplate = (rowData) => {
        const badgeStyle = {
            padding: '0.25rem 0.5rem',
            borderRadius: '0.25rem',
            fontSize: '0.875rem',
            fontWeight: '600',
            color: 'white',
            backgroundColor: rowData.aktif ? '#22c55e' : '#ef4444'
        };

        return (
            <span style={badgeStyle}>
                {rowData.aktif ? 'Aktif' : 'Pasif'}
            </span>
        );
    };

    const odemeTuruBodyTemplate = (rowData) => {
        const turOption = odemeTuruOptions.find(t => t.value === rowData.odemeTuru);
        return turOption ? turOption.label : rowData.odemeTuru;
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <React.Fragment>
                {permissions.update && (
                    <Button
                        icon="pi pi-pencil"
                        className="p-button-rounded p-button-success p-mr-2"
                        onClick={() => editOdeme(rowData)}
                        tooltip="Düzenle"
                    />
                )}
                {permissions.delete && (
                    <Button
                        icon="pi pi-trash"
                        className="p-button-rounded p-button-warning"
                        onClick={() => confirmDeleteOdeme(rowData)}
                        tooltip="Sil"
                    />
                )}
            </React.Fragment>
        );
    };

    const header = (
        <div className="table-header">
            <h5 className="p-m-0">Ödeme Tanımları Yönetimi</h5>
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

    const odemeDialogFooter = (
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
                onClick={saveOdeme}
                disabled={saving}
            />
        </React.Fragment>
    );

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
                    value={odemeler}
                    selection={selectedOdemeler}
                    onSelectionChange={(e) => setSelectedOdemeler(e.value)}
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
                >
                    <Column
                        field="id"
                        header="ID"
                        sortable
                        style={{ minWidth: '4rem', width: '4rem' }}
                    ></Column>
                    <Column
                        field="kod"
                        header="Kod"
                        sortable
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        field="ad"
                        header="Ödeme Adı"
                        sortable
                        style={{ minWidth: '14rem' }}
                    ></Column>
                    <Column
                        field="odemeTuru"
                        header="Ödeme Türü"
                        body={odemeTuruBodyTemplate}
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="aciklama"
                        header="Açıklama"
                        sortable
                        style={{ minWidth: '16rem' }}
                    ></Column>
                    <Column
                        field="aktif"
                        header="Durum"
                        body={statusBodyTemplate}
                        sortable
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        body={actionBodyTemplate}
                        header="İşlemler"
                        style={{ minWidth: '8rem' }}
                    ></Column>
                </DataTable>
            </Card>

            <Dialog
                visible={odemeDialog}
                style={{ width: '550px' }}
                header="Ödeme Tanımı Detayları"
                modal
                className="p-fluid"
                footer={odemeDialogFooter}
                onHide={hideDialog}
            >
                <div className="p-field">
                    <label htmlFor="kod">Kod *</label>
                    <InputText
                        id="kod"
                        value={odeme.kod}
                        onChange={(e) => onInputChange(e, 'kod')}
                        required
                        autoFocus
                        className={submitted && !odeme.kod ? 'p-invalid' : ''}
                        placeholder="Örn: PRIM01, IKRAM01"
                    />
                    {submitted && !odeme.kod && (
                        <small className="p-error">Kod gereklidir.</small>
                    )}
                </div>

                <div className="p-field">
                    <label htmlFor="ad">Ödeme Adı *</label>
                    <InputText
                        id="ad"
                        value={odeme.ad}
                        onChange={(e) => onInputChange(e, 'ad')}
                        required
                        className={submitted && !odeme.ad ? 'p-invalid' : ''}
                        placeholder="Örn: Performans Primi, Yıl Sonu İkramiyesi"
                    />
                    {submitted && !odeme.ad && (
                        <small className="p-error">Ödeme adı gereklidir.</small>
                    )}
                </div>

                <div className="p-field">
                    <label htmlFor="odemeTuru">Ödeme Türü *</label>
                    <Dropdown
                        id="odemeTuru"
                        value={odeme.odemeTuru}
                        options={odemeTuruOptions}
                        onChange={(e) => onInputChange(e, 'odemeTuru')}
                        placeholder="Ödeme türü seçin"
                        className={submitted && !odeme.odemeTuru ? 'p-invalid' : ''}
                    />
                    {submitted && !odeme.odemeTuru && (
                        <small className="p-error">Ödeme türü gereklidir.</small>
                    )}
                </div>

                <div className="p-field">
                    <label htmlFor="aciklama">Açıklama</label>
                    <InputTextarea
                        id="aciklama"
                        value={odeme.aciklama}
                        onChange={(e) => onInputChange(e, 'aciklama')}
                        rows={3}
                        cols={20}
                        placeholder="Ödeme tanımı hakkında detaylı açıklama..."
                    />
                </div>

                <div className="p-field">
                    <label htmlFor="aktif">Durum</label>
                    <div>
                        <InputSwitch
                            id="aktif"
                            checked={odeme.aktif}
                            onChange={(e) => setOdeme({ ...odeme, aktif: e.value })}
                        />
                        <span className="p-ml-2">
                            {odeme.aktif ? 'Aktif' : 'Pasif'}
                        </span>
                    </div>
                </div>
            </Dialog>
        </div>
    );
};

export default OdemeTanimlari;
