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
import departmanService from '../services/departmanService';
import yetkiService from '../services/yetkiService';

const Departmanlar = () => {
    const [departmanlar, setDepartmanlar] = useState([]);
    const [departmanDialog, setDepartmanDialog] = useState(false);
    const [departman, setDepartman] = useState({
        id: null,
        ad: '',
        kod: '',
        aciklama: '',
        aktif: true
    });
    const [selectedDepartmanlar, setSelectedDepartmanlar] = useState(null);
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
        loadDepartmanlar();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('departmanlar', 'read'),
                write: yetkiService.hasScreenPermission('departmanlar', 'write'),
                delete: yetkiService.hasScreenPermission('departmanlar', 'delete'),
                update: yetkiService.hasScreenPermission('departmanlar', 'update')
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

    const loadDepartmanlar = async () => {
        setLoading(true);
        try {
            const response = await departmanService.getAllDepartmanlar();
            if (response.success) {
                setDepartmanlar(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Departmanlar yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const openNew = () => {
        setDepartman({
            id: null,
            ad: '',
            kod: '',
            aciklama: '',
            aktif: true
        });
        setSubmitted(false);
        setDepartmanDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setDepartmanDialog(false);
    };

    const saveDepartman = async () => {
        setSubmitted(true);

        if (departman.ad.trim() && !saving) {
            setSaving(true);
            try {
                // Create a copy without null id for new records
                const dataToSend = { ...departman };
                if (!dataToSend.id) {
                    delete dataToSend.id; // Remove id field for new records
                }
                
            // console.log('Sending departman data:', dataToSend);
                let response;
                if (departman.id) {
                    response = await departmanService.updateDepartman(departman.id, dataToSend);
                } else {
                    response = await departmanService.createDepartman(dataToSend);
                }
            // console.log('Received response:', response);

                if (response.success) {
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: response.message,
                        life: 3000
                    });
                    loadDepartmanlar();
                    setDepartmanDialog(false);
                    setDepartman({
                        id: null,
                        ad: '',
                        kod: '',
                        aciklama: ''
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

    const editDepartman = (departman) => {
        setDepartman({ ...departman });
        setDepartmanDialog(true);
    };

    const confirmDeleteDepartman = (departman) => {
            // console.log('Delete button clicked for departman:', departman);
        confirmDialog({
            message: `${departman.ad} departmanını silmek istediğinizden emin misiniz?`,
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
            // console.log('Delete confirmed for departman ID:', departman.id);
                deleteDepartman(departman.id);
            }
        });
    };

    const deleteDepartman = async (id) => {
            // console.log('deleteDepartman called with ID:', id);
        try {
            const response = await departmanService.deleteDepartman(id);
            // console.log('Delete response:', response);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadDepartmanlar();
            }
        } catch (error) {
            // console.error('Delete error:', error);
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
        let _departman = { ...departman };
        _departman[`${name}`] = val;
        setDepartman(_departman);
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                {permissions.write && (
                    <Button
                        label="Yeni Departman"
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

    const actionBodyTemplate = (rowData) => {
        return (
            <React.Fragment>
                {permissions.update && (
                    <Button
                        icon="pi pi-pencil"
                        className="p-button-rounded p-button-success p-mr-2"
                        onClick={() => editDepartman(rowData)}
                        tooltip="Düzenle"
                    />
                )}
                {permissions.delete && (
                    <Button
                        icon="pi pi-trash"
                        className="p-button-rounded p-button-warning"
                        onClick={() => confirmDeleteDepartman(rowData)}
                        tooltip="Sil"
                    />
                )}
                {!permissions.update && !permissions.delete && (
                    <span className="text-500">Yetki yok</span>
                )}
            </React.Fragment>
        );
    };

    const header = (
        <div className="table-header">
            <h5 className="p-m-0">Departman Yönetimi</h5>
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

    const departmanDialogFooter = (
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
                onClick={saveDepartman}
            />
        </React.Fragment>
    );

    const createdAtBodyTemplate = (rowData) => {
        return new Date(rowData.createdAt).toLocaleDateString('tr-TR');
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
                    value={departmanlar}
                    selection={selectedDepartmanlar}
                    onSelectionChange={(e) => setSelectedDepartmanlar(e.value)}
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
                        field="ad"
                        header="Departman Adı"
                        sortable
                        style={{ minWidth: '12rem' }}
                    ></Column>
                    <Column
                        field="kod"
                        header="Kod"
                        sortable
                        style={{ minWidth: '8rem' }}
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
                visible={departmanDialog}
                style={{ width: '450px' }}
                header="Departman Detayları"
                modal
                className="p-fluid"
                footer={departmanDialogFooter}
                onHide={hideDialog}
            >
                <div className="p-field">
                    <label htmlFor="ad">Departman Adı *</label>
                    <InputText
                        id="ad"
                        value={departman.ad}
                        onChange={(e) => onInputChange(e, 'ad')}
                        required
                        autoFocus
                        className={submitted && !departman.ad ? 'p-invalid' : ''}
                    />
                    {submitted && !departman.ad && (
                        <small className="p-error">Departman adı gereklidir.</small>
                    )}
                </div>

                <div className="p-field">
                    <label htmlFor="kod">Departman Kodu</label>
                    <InputText
                        id="kod"
                        value={departman.kod}
                        onChange={(e) => onInputChange(e, 'kod')}
                        placeholder="Örn: IK, BIT"
                    />
                </div>

                <div className="p-field">
                    <label htmlFor="aciklama">Açıklama</label>
                    <InputTextarea
                        id="aciklama"
                        value={departman.aciklama}
                        onChange={(e) => onInputChange(e, 'aciklama')}
                        rows={3}
                        cols={20}
                    />
                </div>

                <div className="p-field">
                    <label htmlFor="aktif">Durum</label>
                    <div>
                        <InputSwitch
                            id="aktif"
                            checked={departman.aktif}
                            onChange={(e) => setDepartman({ ...departman, aktif: e.value })}
                        />
                        <span className="p-ml-2">
                            {departman.aktif ? 'Aktif' : 'Pasif'}
                        </span>
                    </div>
                </div>
            </Dialog>
        </div>
    );
};

export default Departmanlar;