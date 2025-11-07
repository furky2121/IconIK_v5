import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { InputTextarea } from 'primereact/inputtextarea';
import { Badge } from 'primereact/badge';
import { confirmDialog } from 'primereact/confirmdialog';
import { Card } from 'primereact/card';
import { Chip } from 'primereact/chip';
import { InputNumber } from 'primereact/inputnumber';
import { Dropdown } from 'primereact/dropdown';
import masrafService from '../services/masrafService';
import authService from '../services/authService';
import yetkiService from '../services/yetkiService';

const MasrafTalepleri = () => {
    const [masrafTalepleri, setMasrafTalepleri] = useState([]);
    const [masrafDialog, setMasrafDialog] = useState(false);
    const [masrafTalebi, setMasrafTalebi] = useState({
        id: null,
        personelId: null,
        masrafTipi: null,
        tutar: 0,
        aciklama: ''
    });
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
    const [masrafLimiti, setMasrafLimiti] = useState(null);
    const [currentUser, setCurrentUser] = useState(null);
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
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('masraf-talepleri', 'read'),
                write: yetkiService.hasScreenPermission('masraf-talepleri', 'write'),
                delete: yetkiService.hasScreenPermission('masraf-talepleri', 'delete'),
                update: yetkiService.hasScreenPermission('masraf-talepleri', 'update')
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

    const loadData = async () => {
        const user = authService.getUser();
        if (!user) return;

        setLoading(true);
        try {
            await loadMasrafTalepleri();
        } catch (error) {
            // console.error('Veri yükleme hatası:', error);
        } finally {
            setLoading(false);
        }
    };

    const loadMasrafTalepleri = async () => {
        try {
            const user = authService.getUser();
            const response = await masrafService.getMasrafTalepleri(user.personel.id);
            if (response.success) {
                setMasrafTalepleri(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Masraf talepleri yüklenirken bir hata oluştu.',
                life: 3000
            });
        }
    };

    const loadMasrafLimiti = async (masrafTipi) => {
        try {
            const user = authService.getUser();
            const response = await masrafService.getMasrafLimit(user.personel.id, masrafTipi);
            if (response.success) {
                setMasrafLimiti(response.data);
            }
        } catch (error) {
            // console.error('Masraf limiti yükleme hatası:', error);
            setMasrafLimiti(null);
        }
    };

    const openNew = () => {
        const user = authService.getUser();
        setMasrafTalebi({
            id: null,
            personelId: user.personel.id,
            masrafTipi: null,
            tutar: 0,
            aciklama: ''
        });
        setSubmitted(false);
        setMasrafLimiti(null);
        setMasrafDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setMasrafDialog(false);
        setMasrafLimiti(null);
    };

    const saveMasrafTalebi = async () => {
        setSubmitted(true);

        if (masrafTalebi.masrafTipi && masrafTalebi.tutar && masrafTalebi.tutar > 0) {
            try {
                let response;
                if (masrafTalebi.id) {
                    response = await masrafService.updateMasrafTalebi(masrafTalebi.id, masrafTalebi);
                } else {
                    response = await masrafService.createMasrafTalebi(masrafTalebi);
                }

                if (response.success) {
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: response.message,
                        life: 3000
                    });
                    loadData();
                    setMasrafDialog(false);
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

    const editMasrafTalebi = (masraf) => {
        setMasrafTalebi({ ...masraf });
        if (masraf.masrafTipi) {
            loadMasrafLimiti(masraf.masrafTipi);
        }
        setMasrafDialog(true);
    };

    const confirmDeleteMasraf = (masraf) => {
        confirmDialog({
            message: 'Bu masraf talebini silmek istediğinizden emin misiniz?',
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deleteMasrafTalebi(masraf.id)
        });
    };

    const deleteMasrafTalebi = async (id) => {
        try {
            const response = await masrafService.deleteMasrafTalebi(id);
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

    const onInputChange = (e, name) => {
        const val = (e.target && e.target.value) || e.value || '';
        let _masrafTalebi = { ...masrafTalebi };
        _masrafTalebi[`${name}`] = val;
        setMasrafTalebi(_masrafTalebi);
    };

    const onMasrafTipiChange = (e) => {
        const masrafTipi = e.value;
        setMasrafTalebi({ ...masrafTalebi, masrafTipi });
        
        if (masrafTipi) {
            loadMasrafLimiti(masrafTipi);
        } else {
            setMasrafLimiti(null);
        }
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                {permissions.write && (
                    <Button
                        label="Yeni Masraf Talebi"
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
        const canEdit = rowData.onayDurumu === 'Beklemede' && permissions.update;
        const canDelete = rowData.onayDurumu === 'Beklemede' && permissions.delete;

        return (
            <React.Fragment>
                {canEdit && (
                    <Button
                        icon="pi pi-pencil"
                        className="p-button-rounded p-button-success p-mr-2"
                        onClick={() => editMasrafTalebi(rowData)}
                        tooltip="Düzenle"
                    />
                )}
                {canDelete && (
                    <Button
                        icon="pi pi-trash"
                        className="p-button-rounded p-button-warning"
                        onClick={() => confirmDeleteMasraf(rowData)}
                        tooltip="Sil"
                    />
                )}
            </React.Fragment>
        );
    };

    const durumBodyTemplate = (rowData) => {
        return <Badge value={rowData.onayDurumu} severity={masrafService.getOnayDurumuSeverity(rowData.onayDurumu)}></Badge>;
    };

    const tutarBodyTemplate = (rowData) => {
        return masrafService.formatCurrency(rowData.tutar);
    };

    const tarihBodyTemplate = (rowData) => {
        return masrafService.formatDate(rowData.talepTarihi);
    };

    const masrafTipiBodyTemplate = (rowData) => {
        return (
            <div className="flex align-items-center gap-2">
                <i className={masrafService.getMasrafTuruIcon(rowData.masrafTipi)} style={{color: masrafService.getMasrafTuruColor(rowData.masrafTipi)}}></i>
                <span>{masrafService.getMasrafTuruAdi(rowData.masrafTipi)}</span>
            </div>
        );
    };

    const header = (
        <div className="flex flex-column md:flex-row md:justify-content-between md:align-items-center">
            <h5 className="m-0">Masraf Talepleri</h5>
            <span className="block mt-2 md:mt-0 p-input-icon-left">
                <i className="pi pi-search" />
                <InputText type="search" onInput={(e) => setGlobalFilter(e.target.value)} placeholder="Ara..." />
            </span>
        </div>
    );

    const masrafDialogFooter = (
        <React.Fragment>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label="Kaydet" icon="pi pi-check" className="p-button-text" onClick={saveMasrafTalebi} />
        </React.Fragment>
    );

    if (!permissions.read) {
        return (
            <div className="surface-card p-4 shadow-2 border-round">
                <div className="text-center">
                    <i className="pi pi-lock text-6xl text-red-500 mb-3"></i>
                    <div className="text-xl font-medium text-900 mb-2">Yetkisiz Erişim</div>
                    <div className="text-600 mb-5">Bu sayfayı görüntüleme yetkiniz bulunmamaktadır.</div>
                </div>
            </div>
        );
    }

    return (
        <div className="grid">
            <div className="col-12">
                <Toast ref={toast} />

                {/* Masraf Limit Bilgisi */}
                {masrafLimiti && (
                    <Card className="mb-4">
                        <div className="flex justify-content-between align-items-center">
                            <h6>Masraf Limit Bilginiz ({masrafLimiti.masrafTipiAd} - Bu Ay)</h6>
                            <div className="flex align-items-center gap-2">
                                <Chip 
                                    label={`Limit: ${masrafService.formatCurrency(masrafLimiti.limit)}`} 
                                    className="p-mr-2" 
                                />
                                <Chip 
                                    label={`Kullanılan: ${masrafService.formatCurrency(masrafLimiti.aylikToplam)}`} 
                                    className="p-mr-2 p-chip-warning" 
                                />
                                <Chip 
                                    label={`Kalan: ${masrafService.formatCurrency(masrafLimiti.kalanLimit)}`} 
                                    className="p-chip-success" 
                                />
                            </div>
                        </div>
                    </Card>
                )}

                <Card>
                    <Toolbar
                        className="mb-4"
                        left={leftToolbarTemplate}
                        right={rightToolbarTemplate}
                    ></Toolbar>

                    <DataTable
                        ref={dt}
                        value={masrafTalepleri}
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
                        emptyMessage="Masraf talebi bulunamadı."
                    >
                        <Column
                            field="id"
                            header="ID"
                            sortable
                            style={{ minWidth: '4rem', width: '4rem' }}
                        ></Column>
                        <Column
                            field="talepTarihi"
                            header="Talep Tarihi"
                            body={tarihBodyTemplate}
                            sortable
                            style={{ minWidth: '10rem' }}
                        ></Column>
                        <Column
                            field="masrafTipiAd"
                            header="Masraf Tipi"
                            body={masrafTipiBodyTemplate}
                            sortable
                            style={{ minWidth: '10rem' }}
                        ></Column>
                        <Column
                            field="tutar"
                            header="Tutar"
                            body={tutarBodyTemplate}
                            sortable
                            style={{ minWidth: '10rem' }}
                        ></Column>
                        <Column
                            field="aciklama"
                            header="Açıklama"
                            sortable
                            style={{ minWidth: '15rem' }}
                        ></Column>
                        <Column
                            field="onayDurumu"
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
                            field="onayNotu"
                            header="Onay Notu"
                            sortable
                            style={{ minWidth: '15rem' }}
                        ></Column>
                        <Column
                            body={actionBodyTemplate}
                            header="İşlemler"
                            style={{ minWidth: '8rem' }}
                        ></Column>
                    </DataTable>
                </Card>

                {/* Masraf Talebi Dialog */}
                <Dialog
                    visible={masrafDialog}
                    style={{ width: '500px' }}
                    header="Masraf Talebi Detayları"
                    modal
                    className="p-fluid"
                    footer={masrafDialogFooter}
                    onHide={hideDialog}
                >
                    <div className="field">
                        <label htmlFor="masrafTipi">Masraf Tipi *</label>
                        <Dropdown
                            id="masrafTipi"
                            value={masrafTalebi.masrafTipi}
                            options={masrafService.getMasrafTurleri()}
                            onChange={onMasrafTipiChange}
                            placeholder="Masraf tipini seçin"
                            className={submitted && !masrafTalebi.masrafTipi ? 'p-invalid' : ''}
                            optionTemplate={(option) => (
                                <div className="flex align-items-center gap-2">
                                    <i className={masrafService.getMasrafTuruIcon(option.value)} style={{color: masrafService.getMasrafTuruColor(option.value)}}></i>
                                    <span>{option.label}</span>
                                </div>
                            )}
                        />
                        {submitted && !masrafTalebi.masrafTipi && (
                            <small className="p-error">Masraf tipi seçimi gereklidir.</small>
                        )}
                    </div>

                    <div className="field">
                        <label htmlFor="tutar">Tutar (₺) *</label>
                        <InputNumber
                            id="tutar"
                            value={masrafTalebi.tutar}
                            onValueChange={(e) => onInputChange(e, 'tutar')}
                            mode="currency"
                            currency="TRY"
                            locale="tr"
                            className={submitted && !masrafTalebi.tutar ? 'p-invalid' : ''}
                        />
                        {submitted && !masrafTalebi.tutar && (
                            <small className="p-error">Tutar gereklidir.</small>
                        )}
                        {masrafLimiti && (
                            <small className="p-text-secondary">
                                Bu masraf tipi için kalan limitiniz: {masrafService.formatCurrency(masrafLimiti.kalanLimit)}
                            </small>
                        )}
                    </div>

                    <div className="field">
                        <label htmlFor="aciklama">Açıklama</label>
                        <InputTextarea
                            id="aciklama"
                            value={masrafTalebi.aciklama}
                            onChange={(e) => onInputChange(e, 'aciklama')}
                            rows={3}
                            cols={20}
                        />
                    </div>
                </Dialog>
            </div>
        </div>
    );
};

export default MasrafTalepleri;