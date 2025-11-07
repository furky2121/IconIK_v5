import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { InputTextarea } from 'primereact/inputtextarea';
import { Badge } from 'primereact/badge';
import { Avatar } from 'primereact/avatar';
import { Card } from 'primereact/card';
import { Panel } from 'primereact/panel';
import { Message } from 'primereact/message';
import izinService from '../services/izinService';
import fileUploadService from '../services/fileUploadService';
import authService from '../services/authService';
import yetkiService from '../services/yetkiService';

const BekleyenIzinTalepleri = () => {
    const [bekleyenTalepler, setBekleyenTalepler] = useState([]);
    const [onayDialog, setOnayDialog] = useState(false);
    const [selectedIzin, setSelectedIzin] = useState(null);
    const [onayNotu, setOnayNotu] = useState('');
    const [onayTipi, setOnayTipi] = useState('onayla'); // 'onayla' veya 'reddet'
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
    const [currentUser, setCurrentUser] = useState(null);
    
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
        const user = authService.getUser();
        setCurrentUser(user);
        loadBekleyenTalepler();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('bekleyen-izin-talepleri', 'read'),
                write: yetkiService.hasScreenPermission('bekleyen-izin-talepleri', 'write'),
                delete: yetkiService.hasScreenPermission('bekleyen-izin-talepleri', 'delete'),
                update: yetkiService.hasScreenPermission('bekleyen-izin-talepleri', 'update')
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

    const loadBekleyenTalepler = async () => {
        setLoading(true);
        try {
            const user = authService.getUser();
            const response = await izinService.getBekleyenIzinTalepleri(user.personel.id);

            if (response.success) {
                setBekleyenTalepler(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Bekleyen izin talepleri yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const hideOnayDialog = () => {
        setOnayDialog(false);
        setOnayNotu('');
        setSelectedIzin(null);
    };

    const openOnayDialog = (izin, tip) => {
        setSelectedIzin(izin);
        setOnayTipi(tip);
        setOnayDialog(true);
    };

    const processOnayReddet = async () => {
        if (!selectedIzin) return;

        try {
            const user = authService.getUser();
            let response;

            if (onayTipi === 'onayla') {
                response = await izinService.onaylaIzinTalebi(selectedIzin.id, user.personel.id, onayNotu);
            } else {
                response = await izinService.reddetIzinTalebi(selectedIzin.id, user.personel.id, onayNotu);
            }

            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadBekleyenTalepler();
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
        const user = authService.getUser();
        const canApprove = permissions.update;

        return (
            <React.Fragment>
                {canApprove && (
                    <>
                        <Button
                            icon="pi pi-check"
                            className="p-button-rounded p-button-success p-button-sm p-mr-1"
                            onClick={() => openOnayDialog(rowData, 'onayla')}
                            tooltip="Onayla"
                        />
                        <Button
                            icon="pi pi-times"
                            className="p-button-rounded p-button-danger p-button-sm"
                            onClick={() => openOnayDialog(rowData, 'reddet')}
                            tooltip="Reddet"
                        />
                    </>
                )}
                {!canApprove && (
                    <span className="text-500">Yetki yok</span>
                )}
            </React.Fragment>
        );
    };

    const avatarBodyTemplate = (rowData) => {
        if (rowData.personelFotograf) {
            return (
                <Avatar
                    image={fileUploadService.getAvatarUrl(rowData.personelFotograf)}
                    size="normal"
                    shape="circle"
                />
            );
        } else {
            const names = rowData.personelAd.split(' ');
            return (
                <Avatar
                    label={names[0].charAt(0) + (names[1] ? names[1].charAt(0) : '')}
                    size="normal"
                    shape="circle"
                    style={{ backgroundColor: '#2196F3', color: '#ffffff' }}
                />
            );
        }
    };

    const durumBodyTemplate = (rowData) => {
        return (
            <Badge
                value={rowData.durum}
                severity={izinService.getDurumRengi(rowData.durum)}
            />
        );
    };

    const izinTipiBodyTemplate = (rowData) => {
        return (
            <Badge
                value={rowData.izinTipi}
                severity={izinService.getIzinTipiRengi(rowData.izinTipi)}
            />
        );
    };

    const tarihBodyTemplate = (field) => (rowData) => {
        return izinService.formatTarih(rowData[field]);
    };

    const raporBodyTemplate = (rowData) => {
        if (rowData.raporDosyaYolu) {
            const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';
            const raporUrl = `${apiBaseUrl}${rowData.raporDosyaYolu}`;

            return (
                <a
                    href={raporUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="p-button p-button-sm p-button-info p-button-outlined"
                >
                    <i className="pi pi-file-pdf mr-2"></i>
                    Rapor Görüntüle
                </a>
            );
        }
        return <span className="text-500">Rapor yok</span>;
    };

    const header = (
        <div className="table-header">
            <h5 className="p-m-0">Bekleyen İzin Talepleri</h5>
            <span className="p-input-icon-left">
                <i className="pi pi-search" />
                <input
                    type="search"
                    onInput={(e) => setGlobalFilter(e.target.value)}
                    placeholder="Arama yapın..."
                    className="p-inputtext p-component"
                />
            </span>
        </div>
    );

    const onayDialogFooter = (
        <React.Fragment>
            <Button
                label="İptal"
                icon="pi pi-times"
                className="p-button-text"
                onClick={hideOnayDialog}
            />
            <Button
                label={onayTipi === 'onayla' ? 'Onayla' : 'Reddet'}
                icon={onayTipi === 'onayla' ? 'pi pi-check' : 'pi pi-times'}
                className={onayTipi === 'onayla' ? 'p-button-success' : 'p-button-danger'}
                onClick={processOnayReddet}
            />
        </React.Fragment>
    );

    // Permission kontrolü - Eğer okuma yetkisi yoksa erişim engelle
    if (!permissions.read) {
        return (
            <div className="grid">
                <div className="col-12">
                    <Card>
                        <Message 
                            severity="warn" 
                            text="Bu sayfaya erişim yetkiniz bulunmamaktadır." 
                            className="w-full"
                        />
                    </Card>
                </div>
            </div>
        );
    }

    return (
        <div className="grid">
            <Toast ref={toast} />
            
            <div className="col-12">
                <Card title="Onay Bekleyen İzin İşlemleri" className="shadow-2">
                    <div className="flex justify-content-between align-items-center mb-4">
                        <div className="flex align-items-center gap-3">
                            <i className="pi pi-clock text-orange-500 text-2xl"></i>
                            <div>
                                <p className="m-0 text-600">
                                    Yetkili olduğunuz personellerin bekleyen izin taleplerini görüntüleyebilir ve onaylayabilir/reddedebilirsiniz.
                                </p>
                            </div>
                        </div>
                        <Badge 
                            value={`${bekleyenTalepler.length} Talep`} 
                            severity="warning" 
                            size="large"
                        />
                    </div>

                    <Toolbar
                        className="mb-4"
                        right={rightToolbarTemplate}
                    ></Toolbar>

                    <DataTable
                        ref={dt}
                        value={bekleyenTalepler}
                        dataKey="id"
                        paginator
                        rows={10}
                        rowsPerPageOptions={[5, 10, 25]}
                        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                        currentPageReportTemplate="{first} - {last} arası, toplam {totalRecords} kayıt"
                        globalFilter={globalFilter}
                        header={
                            <div className="flex justify-content-between align-items-center">
                                <h5 className="m-0">Bekleyen Talepler</h5>
                                <span className="p-input-icon-left">
                                    <i className="pi pi-search" />
                                    <input
                                        type="search"
                                        onInput={(e) => setGlobalFilter(e.target.value)}
                                        placeholder="Arama yapın..."
                                        className="p-inputtext p-component"
                                    />
                                </span>
                            </div>
                        }
                        responsiveLayout="scroll"
                        loading={loading}
                        emptyMessage="Bekleyen izin talebi bulunamadı."
                        className="p-datatable-striped"
                    >
                        <Column
                            field="id"
                            header="ID"
                            sortable
                            style={{ minWidth: '4rem', width: '4rem' }}
                        ></Column>
                        <Column
                            body={avatarBodyTemplate}
                            header="Personel"
                            style={{ width: '80px' }}
                        ></Column>
                        <Column
                            field="personelAd"
                            header="Ad Soyad"
                            sortable
                            style={{ width: '150px' }}
                        ></Column>
                        <Column
                            field="personelDepartman"
                            header="Departman"
                            sortable
                            style={{ width: '120px' }}
                        ></Column>
                        <Column
                            field="personelKademe"
                            header="Kademe"
                            sortable
                            style={{ width: '100px' }}
                        ></Column>
                        <Column
                            field="izinTipi"
                            header="İzin Tipi"
                            body={izinTipiBodyTemplate}
                            sortable
                            style={{ width: '120px' }}
                        ></Column>
                        <Column
                            field="izinBaslamaTarihi"
                            header="İzin Başlangıç Tarih/Saati"
                            body={tarihBodyTemplate('izinBaslamaTarihi')}
                            sortable
                            style={{ width: '160px' }}
                        ></Column>
                        <Column
                            field="isbasiTarihi"
                            header="İşbaşı Tarih/Saati"
                            body={tarihBodyTemplate('isbasiTarihi')}
                            sortable
                            style={{ width: '160px' }}
                        ></Column>
                        <Column
                            field="gunSayisi"
                            header="Gün"
                            sortable
                            style={{ width: '80px' }}
                            className="text-center"
                        ></Column>
                        <Column
                            field="durum"
                            header="Durum"
                            body={durumBodyTemplate}
                            sortable
                            style={{ width: '100px' }}
                        ></Column>
                        <Column
                            body={raporBodyTemplate}
                            header="Rapor"
                            style={{ width: '140px' }}
                            className="text-center"
                        ></Column>
                        <Column
                            body={actionBodyTemplate}
                            header="İşlemler"
                            style={{ width: '120px' }}
                        ></Column>
                    </DataTable>
                </Card>
            </div>

            {/* Onay/Reddet Dialog */}
            <Dialog
                visible={onayDialog}
                style={{ width: '500px' }}
                header={`İzin Talebi ${onayTipi === 'onayla' ? 'Onaylama' : 'Reddetme'}`}
                modal
                className="p-fluid"
                footer={onayDialogFooter}
                onHide={hideOnayDialog}
            >
                {selectedIzin && (
                    <div className="grid">
                        <div className="col-12">
                            <div className="surface-card p-4 border-round mb-4">
                                <div className="flex align-items-center gap-3 mb-3">
                                    {avatarBodyTemplate(selectedIzin)}
                                    <div>
                                        <div className="text-xl font-medium">{selectedIzin.personelAd}</div>
                                        <div className="text-600">{selectedIzin.personelDepartman} - {selectedIzin.personelKademe}</div>
                                    </div>
                                </div>
                                
                                <div className="grid">
                                    <div className="col-6">
                                        <div className="text-sm text-600">İzin Tipi</div>
                                        <div className="font-medium">{selectedIzin.izinTipi}</div>
                                    </div>
                                    <div className="col-6">
                                        <div className="text-sm text-600">Gün Sayısı</div>
                                        <div className="font-medium">{selectedIzin.gunSayisi} gün</div>
                                    </div>
                                    <div className="col-6">
                                        <div className="text-sm text-600">İzin Başlangıç Tarih/Saati</div>
                                        <div className="font-medium">{izinService.formatTarih(selectedIzin.izinBaslamaTarihi)}</div>
                                    </div>
                                    <div className="col-6">
                                        <div className="text-sm text-600">İşbaşı Tarih/Saati</div>
                                        <div className="font-medium">{izinService.formatTarih(selectedIzin.isbasiTarihi)}</div>
                                    </div>
                                    {selectedIzin.aciklama && (
                                        <div className="col-12">
                                            <div className="text-sm text-600">Açıklama</div>
                                            <div className="font-medium">{selectedIzin.aciklama}</div>
                                        </div>
                                    )}
                                </div>
                            </div>
                        </div>

                        <div className="col-12">
                            <div className="field">
                                <label htmlFor="onayNotu" className="font-medium">
                                    {onayTipi === 'onayla' ? 'Onay Notu' : 'Reddet Notu'}
                                </label>
                                <InputTextarea
                                    id="onayNotu"
                                    value={onayNotu}
                                    onChange={(e) => setOnayNotu(e.target.value)}
                                    rows={3}
                                    placeholder={onayTipi === 'onayla' ? 'Onay nedeninizi yazabilirsiniz...' : 'Reddet nedeninizi yazın...'}
                                    className="w-full"
                                />
                            </div>
                        </div>
                    </div>
                )}
            </Dialog>
        </div>
    );
};

export default BekleyenIzinTalepleri;