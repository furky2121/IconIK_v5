import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { InputTextarea } from 'primereact/inputtextarea';
import { Badge } from 'primereact/badge';
import { confirmDialog } from 'primereact/confirmdialog';
import { Card } from 'primereact/card';
import { InputText } from 'primereact/inputtext';
import { Chip } from 'primereact/chip';
import istifaService from '../services/istifaService';
import authService from '../services/authService';
import yetkiService from '../services/yetkiService';

const IstifaOnay = () => {
    const [istifaTalepleri, setIstifaTalepleri] = useState([]);
    const [onayDialog, setOnayDialog] = useState(false);
    const [selectedIstifa, setSelectedIstifa] = useState(null);
    const [onayNotu, setOnayNotu] = useState('');
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
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
                read: yetkiService.hasScreenPermission('istifa-onay', 'read'),
                write: yetkiService.hasScreenPermission('istifa-onay', 'write'),
                delete: yetkiService.hasScreenPermission('istifa-onay', 'delete'),
                update: yetkiService.hasScreenPermission('istifa-onay', 'update')
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
            const response = await istifaService.getOnayBekleyenIstifaTalepleri(user.personel.id);
            if (response.success) {
                setIstifaTalepleri(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Onay bekleyen istifa talepleri yüklenirken bir hata oluştu.',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const openOnayDialog = (istifa, onayTipi) => {
        setSelectedIstifa({ ...istifa, onayTipi });
        setOnayNotu('');
        setOnayDialog(true);
    };

    const hideOnayDialog = () => {
        setOnayDialog(false);
        setSelectedIstifa(null);
        setOnayNotu('');
    };

    const confirmOnay = () => {
        const message = selectedIstifa.onayTipi === 'onayla' 
            ? 'Bu istifa talebini onaylamak istediğinizden emin misiniz? Onaylanan talep sonrasında personel pasif duruma geçirilecektir.' 
            : 'Bu istifa talebini reddetmek istediğinizden emin misiniz?';
            
        confirmDialog({
            message: message,
            header: 'Onay İşlemi',
            icon: 'pi pi-exclamation-triangle',
            accept: () => processOnay()
        });
    };

    const processOnay = async () => {
        try {
            const user = authService.getUser();
            const onayData = {
                onaylayanId: user.personel.id,
                onayNotu: onayNotu
            };

            let response;
            if (selectedIstifa.onayTipi === 'onayla') {
                response = await istifaService.onaylaIstifaTalebi(selectedIstifa.id, onayData);
            } else {
                response = await istifaService.reddetIstifaTalebi(selectedIstifa.id, onayData);
            }

            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadData();
                hideOnayDialog();
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
        const canApprove = permissions.update;

        return (
            <React.Fragment>
                {canApprove && (
                    <>
                        <Button
                            icon="pi pi-check"
                            className="p-button-rounded p-button-success p-mr-2"
                            onClick={() => openOnayDialog(rowData, 'onayla')}
                            tooltip="Onayla"
                        />
                        <Button
                            icon="pi pi-times"
                            className="p-button-rounded p-button-danger"
                            onClick={() => openOnayDialog(rowData, 'reddet')}
                            tooltip="Reddet"
                        />
                    </>
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

    const tarihBodyTemplate = (rowData) => {
        return istifaService.formatTarih(rowData.istifaTarihi);
    };

    const sonCalismaTarihiBodyTemplate = (rowData) => {
        return istifaService.formatTarih(rowData.sonCalismaTarihi);
    };

    const calismaYiliBodyTemplate = (rowData) => {
        return `${rowData.calismaYili} yıl`;
    };

    const header = (
        <div className="flex flex-column md:flex-row md:justify-content-between md:align-items-center">
            <h5 className="m-0">Onay Bekleyen İstifa Talepleri</h5>
            <span className="block mt-2 md:mt-0 p-input-icon-left">
                <i className="pi pi-search" />
                <InputText type="search" onInput={(e) => setGlobalFilter(e.target.value)} placeholder="Ara..." />
            </span>
        </div>
    );

    const onayDialogFooter = (
        <React.Fragment>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideOnayDialog} />
            <Button 
                label={selectedIstifa?.onayTipi === 'onayla' ? 'Onayla' : 'Reddet'} 
                icon={selectedIstifa?.onayTipi === 'onayla' ? 'pi pi-check' : 'pi pi-times'} 
                className={selectedIstifa?.onayTipi === 'onayla' ? 'p-button-success' : 'p-button-danger'} 
                onClick={confirmOnay} 
            />
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

                <Card>
                    <Toolbar
                        className="mb-4"
                        right={rightToolbarTemplate}
                    ></Toolbar>

                    <DataTable
                        ref={dt}
                        value={istifaTalepleri}
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
                        emptyMessage="Onay bekleyen istifa talebi bulunamadı."
                    >
                        <Column
                            field="id"
                            header="ID"
                            sortable
                            style={{ minWidth: '4rem', width: '4rem' }}
                        ></Column>
                        <Column
                            field="personelAd"
                            header="Personel"
                            sortable
                            style={{ minWidth: '12rem' }}
                        ></Column>
                        <Column
                            field="personelPozisyon"
                            header="Pozisyon"
                            sortable
                            style={{ minWidth: '10rem' }}
                        ></Column>
                        <Column
                            field="personelDepartman"
                            header="Departman"
                            sortable
                            style={{ minWidth: '10rem' }}
                        ></Column>
                        <Column
                            field="personelIseBaslama"
                            header="İşe Başlama"
                            body={(rowData) => istifaService.formatTarih(rowData.personelIseBaslama)}
                            sortable
                            style={{ minWidth: '10rem' }}
                        ></Column>
                        <Column
                            field="calismaYili"
                            header="Çalışma Yılı"
                            body={calismaYiliBodyTemplate}
                            sortable
                            style={{ minWidth: '8rem' }}
                        ></Column>
                        <Column
                            field="istifaTarihi"
                            header="İstifa Tarihi"
                            body={tarihBodyTemplate}
                            sortable
                            style={{ minWidth: '10rem' }}
                        ></Column>
                        <Column
                            field="sonCalismaTarihi"
                            header="Son Çalışma"
                            body={sonCalismaTarihiBodyTemplate}
                            sortable
                            style={{ minWidth: '10rem' }}
                        ></Column>
                        <Column
                            field="istifaNedeni"
                            header="İstifa Nedeni"
                            sortable
                            style={{ minWidth: '15rem' }}
                        ></Column>
                        <Column
                            body={actionBodyTemplate}
                            header="İşlemler"
                            style={{ minWidth: '10rem' }}
                        ></Column>
                    </DataTable>
                </Card>

                {/* Onay Dialog */}
                <Dialog
                    visible={onayDialog}
                    style={{ width: '500px' }}
                    header={`İstifa Talebi ${selectedIstifa?.onayTipi === 'onayla' ? 'Onaylama' : 'Reddetme'}`}
                    modal
                    className="p-fluid"
                    footer={onayDialogFooter}
                    onHide={hideOnayDialog}
                >
                    {selectedIstifa && (
                        <>
                            <div className="field">
                                <label><strong>Personel:</strong> {selectedIstifa.personelAd}</label>
                            </div>
                            <div className="field">
                                <label><strong>Pozisyon:</strong> {selectedIstifa.personelPozisyon}</label>
                            </div>
                            <div className="field">
                                <label><strong>Departman:</strong> {selectedIstifa.personelDepartman}</label>
                            </div>
                            <div className="field">
                                <div className="flex align-items-center gap-2">
                                    <Chip 
                                        label={`İşe Başlama: ${istifaService.formatTarih(selectedIstifa.personelIseBaslama)}`} 
                                        className="p-mr-2" 
                                    />
                                    <Chip 
                                        label={`Çalışma Yılı: ${selectedIstifa.calismaYili} yıl`} 
                                        className="p-chip-success" 
                                    />
                                </div>
                            </div>
                            <div className="field">
                                <label><strong>Son Çalışma Tarihi:</strong> {istifaService.formatTarih(selectedIstifa.sonCalismaTarihi)}</label>
                            </div>
                            <div className="field">
                                <label><strong>İstifa Nedeni:</strong> {selectedIstifa.istifaNedeni || 'Kişisel nedenlerimden dolayı'}</label>
                            </div>
                            
                            {selectedIstifa.onayTipi === 'onayla' && (
                                <div className="p-message p-message-warn">
                                    <div className="p-message-wrapper">
                                        <span className="p-message-icon pi pi-exclamation-triangle"></span>
                                        <span className="p-message-text">
                                            <strong>Uyarı:</strong> İstifa talebi onaylandığında personel otomatik olarak pasif duruma geçirilecektir.
                                        </span>
                                    </div>
                                </div>
                            )}
                            
                            <div className="field">
                                <label htmlFor="onayNotu">Onay Notu</label>
                                <InputTextarea
                                    id="onayNotu"
                                    value={onayNotu}
                                    onChange={(e) => setOnayNotu(e.target.value)}
                                    rows={3}
                                    cols={20}
                                    placeholder={`${selectedIstifa?.onayTipi === 'onayla' ? 'Onay' : 'Red'} nedenini belirtiniz...`}
                                />
                            </div>
                        </>
                    )}
                </Dialog>
            </div>
        </div>
    );
};

export default IstifaOnay;