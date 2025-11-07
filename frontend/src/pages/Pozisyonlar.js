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
import { InputNumber } from 'primereact/inputnumber';
import { confirmDialog } from 'primereact/confirmdialog';
import { Card } from 'primereact/card';
import { Badge } from 'primereact/badge';
import { InputSwitch } from 'primereact/inputswitch';
import pozisyonService from '../services/pozisyonService';
import departmanService from '../services/departmanService';
import kademeService from '../services/kademeService';
import yetkiService from '../services/yetkiService';

const Pozisyonlar = () => {
    const [pozisyonlar, setPozisyonlar] = useState([]);
    const [departmanlar, setDepartmanlar] = useState([]);
    const [kademeler, setKademeler] = useState([]);
    const [pozisyonDialog, setPozisyonDialog] = useState(false);
    const [pozisyon, setPozisyon] = useState({
        id: null,
        ad: '',
        departmanId: null,
        kademeId: null,
        minMaas: null,
        maxMaas: null,
        aciklama: '',
        aktif: true
    });
    const [selectedPozisyonlar, setSelectedPozisyonlar] = useState(null);
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState('');
    const [loading, setLoading] = useState(false);
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
        loadData();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('pozisyonlar', 'read'),
                write: yetkiService.hasScreenPermission('pozisyonlar', 'write'),
                delete: yetkiService.hasScreenPermission('pozisyonlar', 'delete'),
                update: yetkiService.hasScreenPermission('pozisyonlar', 'update')
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
        await Promise.all([
            loadPozisyonlar(),
            loadDepartmanlar(),
            loadKademeler()
        ]);
    };

    const loadPozisyonlar = async () => {
        setLoading(true);
        try {
            const response = await pozisyonService.getAllPozisyonlar();
            if (response.success) {
                setPozisyonlar(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Pozisyonlar yüklenirken hata oluştu',
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
            // console.error('Departmanlar yüklenirken hata:', error);
        }
    };

    const loadKademeler = async () => {
        try {
            // Dropdown için sadece aktif kademeleri getir
            const response = await kademeService.getAktifKademeler();
            if (response.success) {
                setKademeler(response.data);
            }
        } catch (error) {
            // console.error('Kademeler yüklenirken hata:', error);
        }
    };

    const openNew = () => {
        setPozisyon({
            id: null,
            ad: '',
            departmanId: null,
            kademeId: null,
            minMaas: null,
            maxMaas: null,
            aciklama: '',
            aktif: true
        });
        setSubmitted(false);
        setPozisyonDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setPozisyonDialog(false);
    };

    const savePozisyon = async () => {
        setSubmitted(true);

        if (pozisyon.ad.trim() && pozisyon.departmanId && pozisyon.kademeId) {
            // Maaş kontrolü
            if (pozisyon.minMaas && pozisyon.maxMaas && pozisyon.minMaas > pozisyon.maxMaas) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: 'Minimum maaş, maksimum maaştan büyük olamaz',
                    life: 3000
                });
                return;
            }

            try {
                // Create a copy without null id for new records
                const dataToSend = { ...pozisyon };
                if (!dataToSend.id) {
                    delete dataToSend.id; // Remove id field for new records
                }

            // console.log('Pozisyon data being sent to API:', JSON.stringify(dataToSend, null, 2));

                let response;
                if (pozisyon.id) {
                    response = await pozisyonService.updatePozisyon(pozisyon.id, dataToSend);
                } else {
                    response = await pozisyonService.createPozisyon(dataToSend);
                }

                if (response.success) {
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: response.message,
                        life: 3000
                    });
                    loadPozisyonlar();
                    setPozisyonDialog(false);
                    setPozisyon({
                        id: null,
                        ad: '',
                        departmanId: null,
                        kademeId: null,
                        minMaas: null,
                        maxMaas: null,
                        aciklama: '',
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
            }
        }
    };

    const editPozisyon = (pozisyon) => {
        setPozisyon({ ...pozisyon });
        setPozisyonDialog(true);
    };

    const confirmDeletePozisyon = (pozisyon) => {
        confirmDialog({
            message: `${pozisyon.ad} pozisyonunu silmek istediğinizden emin misiniz?`,
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deletePozisyon(pozisyon.id)
        });
    };

    const deletePozisyon = async (id) => {
        try {
            const response = await pozisyonService.deletePozisyon(id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadPozisyonlar();
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
        let _pozisyon = { ...pozisyon };
        _pozisyon[`${name}`] = val;
        setPozisyon(_pozisyon);
    };

    const onDropdownChange = (e, name) => {
        const val = e.value;
        let _pozisyon = { ...pozisyon };
        _pozisyon[`${name}`] = val;
        setPozisyon(_pozisyon);
    };

    const onNumberChange = (e, name) => {
        const val = e.value || null;
        let _pozisyon = { ...pozisyon };
        _pozisyon[`${name}`] = val;
        setPozisyon(_pozisyon);
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                {permissions.write && (
                    <Button
                        label="Yeni Pozisyon"
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

    const actionBodyTemplate = (rowData) => {
        return (
            <React.Fragment>
                {permissions.update && (
                    <Button
                        icon="pi pi-pencil"
                        className="p-button-rounded p-button-success p-mr-2"
                        onClick={() => editPozisyon(rowData)}
                        tooltip="Düzenle"
                    />
                )}
                {permissions.delete && (
                    <Button
                        icon="pi pi-trash"
                        className="p-button-rounded p-button-warning"
                        onClick={() => confirmDeletePozisyon(rowData)}
                        tooltip="Sil"
                    />
                )}
                {!permissions.update && !permissions.delete && (
                    <span className="text-500">Yetki yok</span>
                )}
            </React.Fragment>
        );
    };

    const maasBodyTemplate = (rowData) => {
        if (rowData.minMaas && rowData.maxMaas) {
            return `₺${rowData.minMaas.toLocaleString()} - ₺${rowData.maxMaas.toLocaleString()}`;
        } else if (rowData.minMaas) {
            return `₺${rowData.minMaas.toLocaleString()}+`;
        } else if (rowData.maxMaas) {
            return `₺${rowData.maxMaas.toLocaleString()}'e kadar`;
        }
        return '-';
    };

    const kademeBodyTemplate = (rowData) => {
        const kademe = kademeler.find(k => k.ad === rowData.kademeAd);
        let severity = 'info';
        
        if (kademe) {
            if (kademe.seviye === 1) {
                severity = 'danger'; // Genel Müdür
            } else if (kademe.seviye <= 3) {
                severity = 'warning'; // Direktör, Grup Müdürü
            } else if (kademe.seviye <= 5) {
                severity = 'success'; // Müdür, Yönetici
            }
        }

        return (
            <Badge
                value={rowData.kademeAd}
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

    const header = (
        <div className="table-header">
            <h5 className="p-m-0">Pozisyon Yönetimi</h5>
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

    const pozisyonDialogFooter = (
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
                onClick={savePozisyon}
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
                    value={pozisyonlar}
                    selection={selectedPozisyonlar}
                    onSelectionChange={(e) => setSelectedPozisyonlar(e.value)}
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
                    emptyMessage="Pozisyon bulunamadı."
                >
                    <Column
                        field="id"
                        header="ID"
                        sortable
                        style={{ minWidth: '4rem', width: '4rem' }}
                    ></Column>
                    <Column
                        field="ad"
                        header="Pozisyon Adı"
                        sortable
                        style={{ minWidth: '14rem' }}
                    ></Column>
                    <Column
                        field="departmanAd"
                        header="Departman"
                        sortable
                        style={{ minWidth: '12rem' }}
                    ></Column>
                    <Column
                        field="kademeAd"
                        header="Kademe"
                        body={kademeBodyTemplate}
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        header="Maaş Aralığı"
                        body={maasBodyTemplate}
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
                visible={pozisyonDialog}
                style={{ width: '450px' }}
                header="Pozisyon Detayları"
                modal
                className="p-fluid"
                footer={pozisyonDialogFooter}
                onHide={hideDialog}
            >
                <div className="p-field">
                    <label htmlFor="ad">Pozisyon Adı *</label>
                    <InputText
                        id="ad"
                        value={pozisyon.ad}
                        onChange={(e) => onInputChange(e, 'ad')}
                        required
                        autoFocus
                        className={submitted && !pozisyon.ad ? 'p-invalid' : ''}
                    />
                    {submitted && !pozisyon.ad && (
                        <small className="p-error">Pozisyon adı gereklidir.</small>
                    )}
                </div>

                <div className="p-field">
                    <label htmlFor="departmanId">Departman *</label>
                    <Dropdown
                        id="departmanId"
                        value={pozisyon.departmanId}
                        options={departmanlar}
                        onChange={(e) => onDropdownChange(e, 'departmanId')}
                        optionLabel="ad"
                        optionValue="id"
                        placeholder="Departman seçiniz"
                        className={submitted && !pozisyon.departmanId ? 'p-invalid' : ''}
                    />
                    {submitted && !pozisyon.departmanId && (
                        <small className="p-error">Departman seçimi gereklidir.</small>
                    )}
                </div>

                <div className="p-field">
                    <label htmlFor="kademeId">Kademe *</label>
                    <Dropdown
                        id="kademeId"
                        value={pozisyon.kademeId}
                        options={kademeler}
                        onChange={(e) => onDropdownChange(e, 'kademeId')}
                        optionLabel="ad"
                        optionValue="id"
                        placeholder="Kademe seçiniz"
                        className={submitted && !pozisyon.kademeId ? 'p-invalid' : ''}
                    />
                    {submitted && !pozisyon.kademeId && (
                        <small className="p-error">Kademe seçimi gereklidir.</small>
                    )}
                </div>

                <div className="p-formgrid p-grid">
                    <div className="p-field p-col-6">
                        <label htmlFor="minMaas">Minimum Maaş (₺)</label>
                        <InputNumber
                            id="minMaas"
                            value={pozisyon.minMaas}
                            onValueChange={(e) => onNumberChange(e, 'minMaas')}
                            mode="currency"
                            currency="TRY"
                            locale="tr"
                            currencyDisplay="code"
                            placeholder="0,00"
                        />
                    </div>
                    <div className="p-field p-col-6">
                        <label htmlFor="maxMaas">Maksimum Maaş (₺)</label>
                        <InputNumber
                            id="maxMaas"
                            value={pozisyon.maxMaas}
                            onValueChange={(e) => onNumberChange(e, 'maxMaas')}
                            mode="currency"
                            currency="TRY"
                            locale="tr"
                            currencyDisplay="code"
                            placeholder="0,00"
                        />
                    </div>
                </div>

                <div className="p-field">
                    <label htmlFor="aciklama">Açıklama</label>
                    <InputTextarea
                        id="aciklama"
                        value={pozisyon.aciklama}
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
                            checked={pozisyon.aktif}
                            onChange={(e) => setPozisyon({ ...pozisyon, aktif: e.value })}
                        />
                        <span className="p-ml-2">
                            {pozisyon.aktif ? 'Aktif' : 'Pasif'}
                        </span>
                    </div>
                </div>
            </Dialog>
        </div>
    );
};

export default Pozisyonlar;