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
import avansService from '../services/avansService';
import authService from '../services/authService';
import yetkiService from '../services/yetkiService';

const AvansTalepleri = () => {
    const [avansTalepleri, setAvansTalepleri] = useState([]);
    const [avansDialog, setAvansDialog] = useState(false);
    const [avansTalebi, setAvansTalebi] = useState({
        id: null,
        personelId: null,
        talepTutari: 0,
        aciklama: ''
    });
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
    const [avansLimiti, setAvansLimiti] = useState(null);
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
                read: yetkiService.hasScreenPermission('avans-talepleri', 'read'),
                write: yetkiService.hasScreenPermission('avans-talepleri', 'write'),
                delete: yetkiService.hasScreenPermission('avans-talepleri', 'delete'),
                update: yetkiService.hasScreenPermission('avans-talepleri', 'update')
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
            await Promise.all([
                loadAvansTalepleri(),
                loadAvansLimiti()
            ]);
        } catch (error) {
            // console.error('Veri yükleme hatası:', error);
        } finally {
            setLoading(false);
        }
    };

    const loadAvansTalepleri = async () => {
        try {
            const user = authService.getUser();
            const response = await avansService.getAvansTalepleri(user.personel.id);
            if (response.success) {
                setAvansTalepleri(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Avans talepleri yüklenirken bir hata oluştu.',
                life: 3000
            });
        }
    };

    const loadAvansLimiti = async () => {
        try {
            const user = authService.getUser();
            const response = await avansService.getAvansLimit(user.personel.id);
            if (response.success) {
                setAvansLimiti(response.data);
            }
        } catch (error) {
            // console.error('Avans limiti yükleme hatası:', error);
        }
    };

    const openNew = () => {
        const user = authService.getUser();
        setAvansTalebi({
            id: null,
            personelId: user.personel.id,
            talepTutari: 0,
            aciklama: ''
        });
        setSubmitted(false);
        setAvansDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setAvansDialog(false);
    };

    const saveAvansTalebi = async () => {
        setSubmitted(true);

        if (avansTalebi.talepTutari && avansTalebi.talepTutari > 0) {
            try {
                let response;
                if (avansTalebi.id) {
                    response = await avansService.updateAvansTalebi(avansTalebi.id, avansTalebi);
                } else {
                    response = await avansService.createAvansTalebi(avansTalebi);
                }

                if (response.success) {
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: response.message,
                        life: 3000
                    });
                    loadData();
                    setAvansDialog(false);
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

    const editAvansTalebi = (avans) => {
        setAvansTalebi({ ...avans });
        setAvansDialog(true);
    };

    const confirmDeleteAvans = (avans) => {
        confirmDialog({
            message: 'Bu avans talebini silmek istediğinizden emin misiniz?',
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deleteAvansTalebi(avans.id)
        });
    };

    const deleteAvansTalebi = async (id) => {
        try {
            const response = await avansService.deleteAvansTalebi(id);
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
        let _avansTalebi = { ...avansTalebi };
        _avansTalebi[`${name}`] = val;
        setAvansTalebi(_avansTalebi);
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                {permissions.write && (
                    <Button
                        label="Yeni Avans Talebi"
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
                        onClick={() => editAvansTalebi(rowData)}
                        tooltip="Düzenle"
                    />
                )}
                {canDelete && (
                    <Button
                        icon="pi pi-trash"
                        className="p-button-rounded p-button-warning"
                        onClick={() => confirmDeleteAvans(rowData)}
                        tooltip="Sil"
                    />
                )}
            </React.Fragment>
        );
    };

    const durumBodyTemplate = (rowData) => {
        const getDurumSeverity = (durum) => {
            switch (durum) {
                case 'Onaylandı': return 'success';
                case 'Reddedildi': return 'danger';
                case 'Beklemede': return 'warning';
                default: return 'info';
            }
        };

        return <Badge value={rowData.onayDurumu} severity={getDurumSeverity(rowData.onayDurumu)}></Badge>;
    };

    const tutarBodyTemplate = (rowData) => {
        return avansService.formatPara(rowData.talepTutari);
    };

    const tarihBodyTemplate = (rowData) => {
        return avansService.formatTarih(rowData.talepTarihi);
    };

    const header = (
        <div className="flex flex-column md:flex-row md:justify-content-between md:align-items-center">
            <h5 className="m-0">Avans Talepleri</h5>
            <span className="block mt-2 md:mt-0 p-input-icon-left">
                <i className="pi pi-search" />
                <InputText type="search" onInput={(e) => setGlobalFilter(e.target.value)} placeholder="Ara..." />
            </span>
        </div>
    );

    const avansDialogFooter = (
        <React.Fragment>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label="Kaydet" icon="pi pi-check" className="p-button-text" onClick={saveAvansTalebi} />
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

                {/* Avans Limit Bilgisi */}
                {avansLimiti && (
                    <Card className="mb-4">
                        <div className="flex justify-content-between align-items-center">
                            <h6>Avans Limit Bilginiz (Bu Ay)</h6>
                            <div className="flex align-items-center gap-2">
                                <Chip
                                    label={`Limit: ${avansService.formatPara(avansLimiti.maxLimit)}`}
                                    className="p-mr-2"
                                />
                                <Chip
                                    label={`Kullanılan: ${avansService.formatPara(avansLimiti.kullanilanAvans)}`}
                                    className="p-mr-2 p-chip-success"
                                />
                                <Chip
                                    label={`Onay Bekleyen: ${avansService.formatPara(avansLimiti.onayBekleyen)}`}
                                    className="p-mr-2 p-chip-warning"
                                />
                                <Chip
                                    label={`Kalan: ${avansService.formatPara(avansLimiti.kalanLimit)}`}
                                    className="p-chip-info"
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
                        value={avansTalepleri}
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
                        emptyMessage="Avans talebi bulunamadı."
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
                            field="talepTutari"
                            header="Talep Tutarı"
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

                {/* Avans Talebi Dialog */}
                <Dialog
                    visible={avansDialog}
                    style={{ width: '450px' }}
                    header="Avans Talebi Detayları"
                    modal
                    className="p-fluid"
                    footer={avansDialogFooter}
                    onHide={hideDialog}
                >
                    <div className="field">
                        <label htmlFor="talepTutari">Talep Tutarı (₺) *</label>
                        <InputNumber
                            id="talepTutari"
                            value={avansTalebi.talepTutari}
                            onValueChange={(e) => onInputChange(e, 'talepTutari')}
                            mode="currency"
                            currency="TRY"
                            locale="tr"
                            className={submitted && !avansTalebi.talepTutari ? 'p-invalid' : ''}
                        />
                        {submitted && !avansTalebi.talepTutari && (
                            <small className="p-error">Talep tutarı gereklidir.</small>
                        )}
                        {avansLimiti && (
                            <small className="p-text-secondary">
                                Maksimum talep edebileceğiniz tutar: {avansService.formatPara(avansLimiti.kalanLimit)}
                            </small>
                        )}
                    </div>

                    <div className="field">
                        <label htmlFor="aciklama">Açıklama</label>
                        <InputTextarea
                            id="aciklama"
                            value={avansTalebi.aciklama}
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

export default AvansTalepleri;