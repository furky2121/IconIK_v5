import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { InputNumber } from 'primereact/inputnumber';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { confirmDialog } from 'primereact/confirmdialog';
import { Card } from 'primereact/card';
import { Badge } from 'primereact/badge';
import { InputSwitch } from 'primereact/inputswitch';
import kademeService from '../services/kademeService';
import yetkiService from '../services/yetkiService';

const Kademeler = () => {
    const [kademeler, setKademeler] = useState([]);
    const [kademeDialog, setKademeDialog] = useState(false);
    const [kademe, setKademe] = useState({
        id: null,
        ad: '',
        seviye: 1,
        aktif: true
    });
    const [selectedKademeler, setSelectedKademeler] = useState(null);
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
    const [saving, setSaving] = useState(false);
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
        loadKademeler();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('kademeler', 'read'),
                write: yetkiService.hasScreenPermission('kademeler', 'write'),
                delete: yetkiService.hasScreenPermission('kademeler', 'delete'),
                update: yetkiService.hasScreenPermission('kademeler', 'update')
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

    const loadKademeler = async () => {
        setLoading(true);
        try {
            const response = await kademeService.getAllKademeler();
            if (response.success) {
                setKademeler(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Kademeler yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const openNew = () => {
        setKademe({
            id: null,
            ad: '',
            seviye: 1,
            aktif: true
        });
        setSubmitted(false);
        setKademeDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setKademeDialog(false);
    };

    const saveKademe = async () => {
        setSubmitted(true);

        if (kademe.ad.trim() && kademe.seviye && !saving) {
            setSaving(true);
            try {
                // Create a copy without null id for new records
                const dataToSend = { ...kademe };
                if (!dataToSend.id) {
                    delete dataToSend.id; // Remove id field for new records
                }
                
                let response;
                if (kademe.id) {
                    response = await kademeService.updateKademe(kademe.id, dataToSend);
                } else {
                    response = await kademeService.createKademe(dataToSend);
                }

                if (response.success) {
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: response.message,
                        life: 3000
                    });
                    loadKademeler();
                    setKademeDialog(false);
                    setKademe({
                        id: null,
                        ad: '',
                        seviye: 1,
                        aktif: true
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

    const editKademe = (kademe) => {
        setKademe({ ...kademe });
        setKademeDialog(true);
    };

    const confirmDeleteKademe = (kademe) => {
        confirmDialog({
            message: `${kademe.ad} kademesini silmek istediğinizden emin misiniz?`,
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                deleteKademe(kademe.id);
            }
        });
    };

    const deleteKademe = async (id) => {
        try {
            const response = await kademeService.deleteKademe(id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadKademeler();
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
        let _kademe = { ...kademe };
        _kademe[`${name}`] = val;
        setKademe(_kademe);
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                {permissions.write && (
                    <Button
                        label="Yeni Kademe"
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

    const seviyeBodyTemplate = (rowData) => {
        let severity = 'info';
        
        if (rowData.seviye === 1) {
            severity = 'danger'; // Genel Müdür
        } else if (rowData.seviye <= 3) {
            severity = 'warning'; // Direktör, Grup Müdürü
        } else if (rowData.seviye <= 5) {
            severity = 'success'; // Müdür, Yönetici
        } else {
            severity = 'info'; // Diğer seviyeler
        }

        return (
            <Badge
                value={`Seviye ${rowData.seviye}`}
                severity={severity}
            />
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

    const actionBodyTemplate = (rowData) => {
        return (
            <React.Fragment>
                {permissions.update && (
                    <Button
                        icon="pi pi-pencil"
                        className="p-button-rounded p-button-success p-mr-2"
                        onClick={() => editKademe(rowData)}
                        tooltip="Düzenle"
                    />
                )}
                {permissions.delete && (
                    <Button
                        icon="pi pi-trash"
                        className="p-button-rounded p-button-warning"
                        onClick={() => confirmDeleteKademe(rowData)}
                        tooltip="Sil"
                    />
                )}
                {!permissions.update && !permissions.delete && (
                    <span className="text-500">Yetki yok</span>
                )}
            </React.Fragment>
        );
    };

    const createdAtBodyTemplate = (rowData) => {
        return new Date(rowData.createdAt).toLocaleDateString('tr-TR');
    };

    const header = (
        <div className="table-header">
            <h5 className="p-m-0">Kademe Yönetimi</h5>
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

    const kademeDialogFooter = (
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
                onClick={saveKademe}
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
                    value={kademeler}
                    selection={selectedKademeler}
                    onSelectionChange={(e) => setSelectedKademeler(e.value)}
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
                        field="seviye"
                        header="Seviye"
                        body={seviyeBodyTemplate}
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="ad"
                        header="Kademe Adı"
                        sortable
                        style={{ minWidth: '12rem' }}
                    ></Column>
                    <Column
                        field="aktif"
                        header="Durum"
                        body={statusBodyTemplate}
                        sortable
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        field="createdAt"
                        header="Oluşturulma Tarihi"
                        body={createdAtBodyTemplate}
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

            <Dialog
                visible={kademeDialog}
                style={{ width: '450px' }}
                header="Kademe Detayları"
                modal
                className="p-fluid"
                footer={kademeDialogFooter}
                onHide={hideDialog}
            >
                <div className="p-field">
                    <label htmlFor="ad">Kademe Adı *</label>
                    <InputText
                        id="ad"
                        value={kademe.ad}
                        onChange={(e) => onInputChange(e, 'ad')}
                        required
                        autoFocus
                        className={submitted && !kademe.ad ? 'p-invalid' : ''}
                    />
                    {submitted && !kademe.ad && (
                        <small className="p-error">Kademe adı gereklidir.</small>
                    )}
                </div>

                <div className="p-field">
                    <label htmlFor="seviye">Seviye *</label>
                    <InputNumber
                        id="seviye"
                        value={kademe.seviye}
                        onValueChange={(e) => setKademe({ ...kademe, seviye: e.value })}
                        min={1}
                        max={20}
                        required
                        className={submitted && !kademe.seviye ? 'p-invalid' : ''}
                    />
                    {submitted && !kademe.seviye && (
                        <small className="p-error">Seviye gereklidir.</small>
                    )}
                </div>

                <div className="p-field">
                    <label htmlFor="aktif">Durum</label>
                    <div>
                        <InputSwitch
                            id="aktif"
                            checked={kademe.aktif}
                            onChange={(e) => setKademe({ ...kademe, aktif: e.value })}
                        />
                        <span className="p-ml-2">
                            {kademe.aktif ? 'Aktif' : 'Pasif'}
                        </span>
                    </div>
                </div>
            </Dialog>
        </div>
    );
};

export default Kademeler;