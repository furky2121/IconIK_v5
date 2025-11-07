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
import { Tag } from 'primereact/tag';
import kesintiTanimiService from '../services/kesintiTanimiService';
import yetkiService from '../services/yetkiService';

const KesintiTanimlari = () => {
    const emptyKesinti = {
        id: null,
        kod: '',
        ad: '',
        kesintiTuru: 'Zorunlu',
        taksitlenebilir: false,
        otomatikHesapla: false,
        aciklama: '',
        aktif: true
    };

    const [kesintiler, setKesintiler] = useState([]);
    const [kesintiDialog, setKesintiDialog] = useState(false);
    const [kesinti, setKesinti] = useState(emptyKesinti);
    const [selectedKesintiler, setSelectedKesintiler] = useState(null);
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

    const kesintiTuruOptions = [
        { label: 'Zorunlu', value: 'Zorunlu' },
        { label: 'İsteğe Bağlı', value: 'IstegeBagli' },
        { label: 'Avans', value: 'Avans' },
        { label: 'İcra', value: 'Icra' },
        { label: 'Kredi', value: 'Kredi' },
        { label: 'Nafaka', value: 'Nafaka' },
        { label: 'Diğer', value: 'Diger' }
    ];

    useEffect(() => {
        loadKesintiler();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('kesinti-tanimlari', 'read'),
                write: yetkiService.hasScreenPermission('kesinti-tanimlari', 'write'),
                delete: yetkiService.hasScreenPermission('kesinti-tanimlari', 'delete'),
                update: yetkiService.hasScreenPermission('kesinti-tanimlari', 'update')
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

    const loadKesintiler = async () => {
        setLoading(true);
        try {
            const response = await kesintiTanimiService.getAll();
            if (response.success) {
                setKesintiler(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Kesinti tanımları yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const openNew = () => {
        setKesinti(emptyKesinti);
        setSubmitted(false);
        setKesintiDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setKesintiDialog(false);
    };

    const saveKesinti = async () => {
        setSubmitted(true);

        if (kesinti.kod.trim() && kesinti.ad.trim() && !saving) {
            setSaving(true);
            try {
                const dataToSend = { ...kesinti };
                if (!dataToSend.id) {
                    delete dataToSend.id;
                }

                let response;
                if (kesinti.id) {
                    response = await kesintiTanimiService.update(kesinti.id, dataToSend);
                } else {
                    response = await kesintiTanimiService.create(dataToSend);
                }

                if (response.success) {
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: response.message,
                        life: 3000
                    });
                    loadKesintiler();
                    setKesintiDialog(false);
                    setKesinti(emptyKesinti);
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

    const editKesinti = (kesinti) => {
        setKesinti({ ...kesinti });
        setKesintiDialog(true);
    };

    const confirmDeleteKesinti = (kesinti) => {
        confirmDialog({
            message: `${kesinti.ad} kesinti tanımını silmek istediğinizden emin misiniz?`,
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deleteKesinti(kesinti.id)
        });
    };

    const deleteKesinti = async (id) => {
        try {
            const response = await kesintiTanimiService.delete(id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadKesintiler();
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
        let _kesinti = { ...kesinti };
        _kesinti[`${name}`] = val;
        setKesinti(_kesinti);
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                {permissions.write && (
                    <Button
                        label="Yeni Kesinti Tanımı"
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

    const kesintiTuruBodyTemplate = (rowData) => {
        const turOption = kesintiTuruOptions.find(t => t.value === rowData.kesintiTuru);
        return turOption ? turOption.label : rowData.kesintiTuru;
    };

    const booleanBodyTemplate = (rowData, field) => {
        return rowData[field] ? (
            <Tag value="Evet" severity="success" />
        ) : (
            <Tag value="Hayır" severity="secondary" />
        );
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <React.Fragment>
                {permissions.update && (
                    <Button
                        icon="pi pi-pencil"
                        className="p-button-rounded p-button-success p-mr-2"
                        onClick={() => editKesinti(rowData)}
                        tooltip="Düzenle"
                    />
                )}
                {permissions.delete && (
                    <Button
                        icon="pi pi-trash"
                        className="p-button-rounded p-button-warning"
                        onClick={() => confirmDeleteKesinti(rowData)}
                        tooltip="Sil"
                    />
                )}
            </React.Fragment>
        );
    };

    const header = (
        <div className="table-header">
            <h5 className="p-m-0">Kesinti Tanımları Yönetimi</h5>
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

    const kesintiDialogFooter = (
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
                onClick={saveKesinti}
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
                    value={kesintiler}
                    selection={selectedKesintiler}
                    onSelectionChange={(e) => setSelectedKesintiler(e.value)}
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
                        header="Kesinti Adı"
                        sortable
                        style={{ minWidth: '14rem' }}
                    ></Column>
                    <Column
                        field="kesintiTuru"
                        header="Kesinti Türü"
                        body={kesintiTuruBodyTemplate}
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="taksitlenebilir"
                        header="Taksitlenebilir"
                        body={(rowData) => booleanBodyTemplate(rowData, 'taksitlenebilir')}
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="otomatikHesapla"
                        header="Otomatik"
                        body={(rowData) => booleanBodyTemplate(rowData, 'otomatikHesapla')}
                        sortable
                        style={{ minWidth: '8rem' }}
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
                visible={kesintiDialog}
                style={{ width: '550px' }}
                header="Kesinti Tanımı Detayları"
                modal
                className="p-fluid"
                footer={kesintiDialogFooter}
                onHide={hideDialog}
            >
                <div className="p-field">
                    <label htmlFor="kod">Kod *</label>
                    <InputText
                        id="kod"
                        value={kesinti.kod}
                        onChange={(e) => onInputChange(e, 'kod')}
                        required
                        autoFocus
                        className={submitted && !kesinti.kod ? 'p-invalid' : ''}
                        placeholder="Örn: AVANS01, ICRA01"
                    />
                    {submitted && !kesinti.kod && (
                        <small className="p-error">Kod gereklidir.</small>
                    )}
                </div>

                <div className="p-field">
                    <label htmlFor="ad">Kesinti Adı *</label>
                    <InputText
                        id="ad"
                        value={kesinti.ad}
                        onChange={(e) => onInputChange(e, 'ad')}
                        required
                        className={submitted && !kesinti.ad ? 'p-invalid' : ''}
                        placeholder="Örn: Avans Kesintisi, İcra Kesintisi"
                    />
                    {submitted && !kesinti.ad && (
                        <small className="p-error">Kesinti adı gereklidir.</small>
                    )}
                </div>

                <div className="p-field">
                    <label htmlFor="kesintiTuru">Kesinti Türü *</label>
                    <Dropdown
                        id="kesintiTuru"
                        value={kesinti.kesintiTuru}
                        options={kesintiTuruOptions}
                        onChange={(e) => onInputChange(e, 'kesintiTuru')}
                        placeholder="Kesinti türü seçin"
                        className={submitted && !kesinti.kesintiTuru ? 'p-invalid' : ''}
                    />
                    {submitted && !kesinti.kesintiTuru && (
                        <small className="p-error">Kesinti türü gereklidir.</small>
                    )}
                </div>

                <div className="p-field">
                    <label htmlFor="taksitlenebilir">Taksitlenebilir</label>
                    <div>
                        <InputSwitch
                            id="taksitlenebilir"
                            checked={kesinti.taksitlenebilir}
                            onChange={(e) => setKesinti({ ...kesinti, taksitlenebilir: e.value })}
                        />
                        <span className="p-ml-2">
                            {kesinti.taksitlenebilir ? 'Evet' : 'Hayır'}
                        </span>
                        <small className="p-d-block p-mt-1 p-text-secondary">
                            Bu kesinti taksitlenebilir mi? (Örn: Avans, Kredi)
                        </small>
                    </div>
                </div>

                <div className="p-field">
                    <label htmlFor="otomatikHesapla">Otomatik Hesapla</label>
                    <div>
                        <InputSwitch
                            id="otomatikHesapla"
                            checked={kesinti.otomatikHesapla}
                            onChange={(e) => setKesinti({ ...kesinti, otomatikHesapla: e.value })}
                        />
                        <span className="p-ml-2">
                            {kesinti.otomatikHesapla ? 'Evet' : 'Hayır'}
                        </span>
                        <small className="p-d-block p-mt-1 p-text-secondary">
                            Bu kesinti otomatik olarak mı hesaplanacak? (Örn: SGK, Vergi)
                        </small>
                    </div>
                </div>

                <div className="p-field">
                    <label htmlFor="aciklama">Açıklama</label>
                    <InputTextarea
                        id="aciklama"
                        value={kesinti.aciklama}
                        onChange={(e) => onInputChange(e, 'aciklama')}
                        rows={3}
                        cols={20}
                        placeholder="Kesinti tanımı hakkında detaylı açıklama..."
                    />
                </div>

                <div className="p-field">
                    <label htmlFor="aktif">Durum</label>
                    <div>
                        <InputSwitch
                            id="aktif"
                            checked={kesinti.aktif}
                            onChange={(e) => setKesinti({ ...kesinti, aktif: e.value })}
                        />
                        <span className="p-ml-2">
                            {kesinti.aktif ? 'Aktif' : 'Pasif'}
                        </span>
                    </div>
                </div>
            </Dialog>
        </div>
    );
};

export default KesintiTanimlari;
