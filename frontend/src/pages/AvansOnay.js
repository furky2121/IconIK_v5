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
import avansService from '../services/avansService';
import authService from '../services/authService';
import yetkiService from '../services/yetkiService';

const AvansOnay = () => {
    const [avansTalepleri, setAvansTalepleri] = useState([]);
    const [onayDialog, setOnayDialog] = useState(false);
    const [selectedAvans, setSelectedAvans] = useState(null);
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
                read: yetkiService.hasScreenPermission('avans-onay', 'read'),
                write: yetkiService.hasScreenPermission('avans-onay', 'write'),
                delete: yetkiService.hasScreenPermission('avans-onay', 'delete'),
                update: yetkiService.hasScreenPermission('avans-onay', 'update')
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
            const response = await avansService.getOnayBekleyenAvansTalepleri(user.personel.id);
            if (response.success) {
                setAvansTalepleri(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Onay bekleyen avans talepleri yüklenirken bir hata oluştu.',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const openOnayDialog = (avans, onayTipi) => {
        setSelectedAvans({ ...avans, onayTipi });
        setOnayNotu('');
        setOnayDialog(true);
    };

    const hideOnayDialog = () => {
        setOnayDialog(false);
        setSelectedAvans(null);
        setOnayNotu('');
    };

    const confirmOnay = () => {
        confirmDialog({
            message: `Bu avans talebini ${selectedAvans.onayTipi === 'onayla' ? 'onaylamak' : 'reddetmek'} istediğinizden emin misiniz?`,
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
            if (selectedAvans.onayTipi === 'onayla') {
                response = await avansService.onaylaAvansTalebi(selectedAvans.id, onayData);
            } else {
                response = await avansService.reddetAvansTalebi(selectedAvans.id, onayData);
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

    const tutarBodyTemplate = (rowData) => {
        return avansService.formatPara(rowData.talepTutari);
    };

    const tarihBodyTemplate = (rowData) => {
        return avansService.formatTarih(rowData.talepTarihi);
    };

    const header = (
        <div className="flex flex-column md:flex-row md:justify-content-between md:align-items-center">
            <h5 className="m-0">Onay Bekleyen Avans Talepleri</h5>
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
                label={selectedAvans?.onayTipi === 'onayla' ? 'Onayla' : 'Reddet'} 
                icon={selectedAvans?.onayTipi === 'onayla' ? 'pi pi-check' : 'pi pi-times'} 
                className={selectedAvans?.onayTipi === 'onayla' ? 'p-button-success' : 'p-button-danger'} 
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
                        emptyMessage="Onay bekleyen avans talebi bulunamadı."
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
                            body={actionBodyTemplate}
                            header="İşlemler"
                            style={{ minWidth: '10rem' }}
                        ></Column>
                    </DataTable>
                </Card>

                {/* Onay Dialog */}
                <Dialog
                    visible={onayDialog}
                    style={{ width: '450px' }}
                    header={`Avans Talebi ${selectedAvans?.onayTipi === 'onayla' ? 'Onaylama' : 'Reddetme'}`}
                    modal
                    className="p-fluid"
                    footer={onayDialogFooter}
                    onHide={hideOnayDialog}
                >
                    {selectedAvans && (
                        <>
                            <div className="field">
                                <label><strong>Personel:</strong> {selectedAvans.personelAd}</label>
                            </div>
                            <div className="field">
                                <label><strong>Talep Tutarı:</strong> {avansService.formatPara(selectedAvans.talepTutari)}</label>
                            </div>
                            <div className="field">
                                <label><strong>Açıklama:</strong> {selectedAvans.aciklama || 'Belirtilmemiş'}</label>
                            </div>
                            <div className="field">
                                <label htmlFor="onayNotu">Onay Notu</label>
                                <InputTextarea
                                    id="onayNotu"
                                    value={onayNotu}
                                    onChange={(e) => setOnayNotu(e.target.value)}
                                    rows={3}
                                    cols={20}
                                    placeholder={`${selectedAvans?.onayTipi === 'onayla' ? 'Onay' : 'Red'} nedenini belirtiniz...`}
                                />
                            </div>
                        </>
                    )}
                </Dialog>
            </div>
        </div>
    );
};

export default AvansOnay;