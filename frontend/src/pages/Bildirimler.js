import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Dropdown } from 'primereact/dropdown';
import { Calendar } from 'primereact/calendar';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { Card } from 'primereact/card';
import { Badge } from 'primereact/badge';
import { Avatar } from 'primereact/avatar';
import { confirmDialog, ConfirmDialog } from 'primereact/confirmdialog';
import { Chip } from 'primereact/chip';
import { Message } from 'primereact/message';
import { Panel } from 'primereact/panel';
import { Divider } from 'primereact/divider';
import { Skeleton } from 'primereact/skeleton';
import { DataView } from 'primereact/dataview';
import { Tag } from 'primereact/tag';
import { SelectButton } from 'primereact/selectbutton';
import notificationService from '../services/notificationService';
import authService from '../services/authService';

const Bildirimler = () => {
    const [notifications, setNotifications] = useState([]);
    const [selectedNotifications, setSelectedNotifications] = useState([]);
    const [notificationDialog, setNotificationDialog] = useState(false);
    const [selectedNotification, setSelectedNotification] = useState(null);
    const [loading, setLoading] = useState(false);
    const [globalFilter, setGlobalFilter] = useState(null);
    const [filters, setFilters] = useState({
        kategori: null,
        tip: null,
        okundu: null,
        tarihBaslangic: null,
        tarihBitis: null
    });
    const [currentUser, setCurrentUser] = useState(null);
    const [viewMode, setViewMode] = useState('table'); // 'table' or 'grid'
    const [stats, setStats] = useState({
        total: 0,
        unread: 0,
        today: 0,
        thisWeek: 0
    });

    const toast = useRef(null);
    const dt = useRef(null);

    const viewOptions = [
        { label: 'Tablo', value: 'table', icon: 'pi pi-table' },
        { label: 'Kart', value: 'grid', icon: 'pi pi-th-large' }
    ];

    // Kategori seçenekleri
    const getKategoriOptions = () => {
        return [
            { label: 'Tümü', value: null },
            { label: 'İzin', value: 'izin' },
            { label: 'Eğitim', value: 'egitim' },
            { label: 'Doğum Günü', value: 'dogum_gunu' },
            { label: 'Sistem', value: 'sistem' },
            { label: 'Avans', value: 'avans' },
            { label: 'İstifa', value: 'istifa' },
            { label: 'Masraf', value: 'masraf' },
            { label: 'Duyuru', value: 'duyuru' },
            { label: 'Anket', value: 'anket' }
        ];
    };

    const getTipOptions = () => {
        return [
            { label: 'Tümü', value: null },
            { label: 'Bilgi', value: 'info' },
            { label: 'Başarılı', value: 'success' },
            { label: 'Uyarı', value: 'warning' },
            { label: 'Hata', value: 'error' }
        ];
    };

    const okunduOptions = [
        { label: 'Tümü', value: null },
        { label: 'Okundu', value: true },
        { label: 'Okunmadı', value: false }
    ];

    useEffect(() => {
        // Client-side kontrolü
        if (typeof window !== 'undefined') {
            const user = authService.getUser();
            if (user) {
                const personel = user.Personel || user.personel;
                setCurrentUser({
                    personelId: personel?.id || personel?.Id || 1
                });
                loadNotifications(personel?.id || personel?.Id || 1);
            }
        }
    }, []);

    const calculateStats = (notificationList) => {
        const now = new Date();
        const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
        const weekAgo = new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000);

        return {
            total: notificationList.length,
            unread: notificationList.filter(n => !n.okundu).length,
            today: notificationList.filter(n => new Date(n.olusturulmaTarihi) >= today).length,
            thisWeek: notificationList.filter(n => new Date(n.olusturulmaTarihi) >= weekAgo).length
        };
    };

    const loadNotifications = async (personelId) => {
        setLoading(true);
        try {
            const result = await notificationService.getAllNotifications(personelId);
            if (result.success) {
                setNotifications(result.data);
                setStats(calculateStats(result.data));
            } else {
                toast.current?.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: 'Bildirimler yüklenirken hata oluştu'
                });
            }
        } catch (error) {
            // console.error('Load notifications error:', error);
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Bildirimler yüklenirken hata oluştu'
            });
        } finally {
            setLoading(false);
        }
    };

    const applyFilters = () => {
        let filtered = [...notifications];

        if (filters.kategori) {
            filtered = filtered.filter(n => n.kategori === filters.kategori);
        }

        if (filters.tip) {
            filtered = filtered.filter(n => n.tip === filters.tip);
        }

        if (filters.okundu !== null) {
            filtered = filtered.filter(n => n.okundu === filters.okundu);
        }

        if (filters.tarihBaslangic) {
            filtered = filtered.filter(n => new Date(n.olusturulmaTarihi) >= filters.tarihBaslangic);
        }

        if (filters.tarihBitis) {
            filtered = filtered.filter(n => new Date(n.olusturulmaTarihi) <= filters.tarihBitis);
        }

        return filtered;
    };

    const clearFilters = () => {
        setFilters({
            kategori: null,
            tip: null,
            okundu: null,
            tarihBaslangic: null,
            tarihBitis: null
        });
        setGlobalFilter(null);
    };

    const markAsRead = async (notification) => {
        if (!notification.okundu) {
            const result = await notificationService.markAsRead(notification.id);
            if (result.success) {
                await loadNotifications(currentUser.personelId);
                toast.current?.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Bildirim okundu olarak işaretlendi'
                });
            }
        }
    };

    const markAllAsRead = async () => {
        const result = await notificationService.markAllAsRead(currentUser.personelId);
        if (result.success) {
            await loadNotifications(currentUser.personelId);
            toast.current?.show({
                severity: 'success',
                summary: 'Başarılı',
                detail: 'Tüm bildirimler okundu olarak işaretlendi'
            });
        }
    };

    const deleteSelected = () => {
        if (selectedNotifications.length === 0) {
            toast.current?.show({
                severity: 'warn',
                summary: 'Uyarı',
                detail: 'Lütfen silmek istediğiniz bildirimleri seçin'
            });
            return;
        }

        confirmDialog({
            message: `${selectedNotifications.length} bildirimi silmek istediğinizden emin misiniz?`,
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: async () => {
                for (const notification of selectedNotifications) {
                    await notificationService.deleteNotification(notification.id);
                }
                await loadNotifications(currentUser.personelId);
                setSelectedNotifications([]);
                toast.current?.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Seçili bildirimler silindi'
                });
            }
        });
    };

    const viewNotification = (notification) => {
        setSelectedNotification(notification);
        setNotificationDialog(true);
        markAsRead(notification);
    };

    const hideDialog = () => {
        setNotificationDialog(false);
        setSelectedNotification(null);
    };

    const navigateToAction = (actionUrl) => {
        if (actionUrl) {
            hideDialog();
            window.location.href = actionUrl;
        }
    };

    // Template functions
    const categoryBodyTemplate = (rowData) => {
        if (!notificationService.getCategoryConfig) {
            return <span>{rowData.kategori}</span>;
        }
        const config = notificationService.getCategoryConfig(rowData.kategori);
        return (
            <Chip
                label={config.label}
                icon={`pi ${config.icon}`}
                style={{
                    backgroundColor: config.color,
                    color: 'white',
                    fontSize: '0.7rem',
                    padding: '0.2rem 0.4rem',
                    height: '20px'
                }}
            />
        );
    };

    const statusBodyTemplate = (rowData) => {
        return (
            <Badge
                value={rowData.okundu ? 'Okundu' : 'Okunmadı'}
                severity={rowData.okundu ? 'success' : 'warning'}
                style={{ fontSize: '0.7rem', padding: '0.2rem 0.4rem' }}
            />
        );
    };

    const dateBodyTemplate = (rowData) => {
        const date = new Date(rowData.olusturulmaTarihi);
        return (
            <span style={{ fontSize: '0.8rem' }}>
                {date.toLocaleDateString('tr-TR', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit'
                })}
            </span>
        );
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-2">
                <Button
                    icon="pi pi-eye"
                    className="p-button-rounded p-button-info p-button-sm"
                    onClick={() => viewNotification(rowData)}
                    tooltip="Görüntüle"
                />
                {!rowData.okundu && (
                    <Button
                        icon="pi pi-check"
                        className="p-button-rounded p-button-success p-button-sm"
                        onClick={() => markAsRead(rowData)}
                        tooltip="Okundu İşaretle"
                    />
                )}
            </div>
        );
    };

    const leftToolbarTemplate = () => {
        return (
            <div className="flex flex-wrap gap-2">
                <Button
                    label="Tümünü Okundu İşaretle"
                    icon="pi pi-check-circle"
                    className="p-button-success"
                    onClick={markAllAsRead}
                />
                <Button
                    label="Seçilileri Sil"
                    icon="pi pi-trash"
                    className="p-button-danger"
                    onClick={deleteSelected}
                    disabled={!selectedNotifications || selectedNotifications.length === 0}
                />
            </div>
        );
    };

    const rightToolbarTemplate = () => {
        const activeFilterCount = Object.values(filters).filter(f => f !== null).length;

        return (
            <div className="flex flex-wrap align-items-center gap-2">
                <SelectButton
                    value={viewMode}
                    onChange={(e) => setViewMode(e.value)}
                    options={viewOptions}
                    optionLabel="label"
                />
                <span className="p-input-icon-left">
                    <i className="pi pi-search" />
                    <InputText
                        type="search"
                        onInput={(e) => setGlobalFilter(e.target.value)}
                        placeholder="Ara..."
                        value={globalFilter || ''}
                    />
                </span>
                <Button
                    icon="pi pi-filter-slash"
                    className="p-button-outlined"
                    onClick={clearFilters}
                    tooltip="Filtreleri Temizle"
                    badge={activeFilterCount > 0 ? activeFilterCount.toString() : null}
                    badgeClassName="p-badge-danger"
                />
            </div>
        );
    };

    const dialogFooter = (
        <div>
            <Button
                label="Kapat"
                icon="pi pi-times"
                onClick={hideDialog}
                className="p-button-text"
            />
            {selectedNotification?.actionUrl && (
                <Button
                    label="Sayfaya Git"
                    icon="pi pi-external-link"
                    onClick={() => navigateToAction(selectedNotification.actionUrl)}
                    autoFocus
                />
            )}
        </div>
    );

    const filteredNotifications = applyFilters();

    // Skeleton loader component
    const renderSkeleton = () => {
        return (
            <div className="grid">
                {[1, 2, 3, 4].map((i) => (
                    <div key={i} className="col-12 md:col-6 lg:col-3">
                        <Card>
                            <Skeleton width="100%" height="80px" className="mb-2" />
                            <Skeleton width="70%" height="20px" />
                        </Card>
                    </div>
                ))}
            </div>
        );
    };

    // Grid görünümü için kart template
    const gridItemTemplate = (notification) => {
        if (!notificationService.getCategoryConfig) {
            return null;
        }

        const config = notificationService.getCategoryConfig(notification.kategori);
        const dateObj = new Date(notification.olusturulmaTarihi);
        const timeAgo = notificationService.formatTimeAgo(notification.olusturulmaTarihi);

        return (
            <div className="col-12 md:col-6 lg:col-4">
                <Card
                    className="notification-card cursor-pointer transition-all transition-duration-300 hover:shadow-5"
                    style={{
                        height: '100%',
                        borderLeft: !notification.okundu ? `4px solid ${config.color}` : '4px solid transparent',
                        backgroundColor: !notification.okundu ? 'var(--surface-50)' : 'white'
                    }}
                    onClick={() => viewNotification(notification)}
                >
                    <div className="flex align-items-start gap-3">
                        <Avatar
                            icon={`pi ${config.icon}`}
                            style={{
                                backgroundColor: config.color,
                                color: 'white',
                                minWidth: '48px',
                                width: '48px',
                                height: '48px'
                            }}
                            size="xlarge"
                            shape="circle"
                        />
                        <div className="flex-1 min-w-0">
                            <div className="flex justify-content-between align-items-start mb-2">
                                <Chip
                                    label={config.label}
                                    icon={`pi ${config.icon}`}
                                    style={{ backgroundColor: config.color, color: 'white' }}
                                    className="text-xs"
                                />
                                {!notification.okundu && (
                                    <div
                                        className="border-circle"
                                        style={{
                                            backgroundColor: config.color,
                                            width: '10px',
                                            height: '10px'
                                        }}
                                    />
                                )}
                            </div>
                            <h6 className={`mt-2 mb-2 ${!notification.okundu ? 'font-bold' : 'font-medium'}`}>
                                {notification.baslik}
                            </h6>
                            <p className="text-sm text-600 line-height-3 mb-3">
                                {notification.mesaj.length > 100
                                    ? notification.mesaj.substring(0, 100) + '...'
                                    : notification.mesaj
                                }
                            </p>
                            <div className="flex justify-content-between align-items-center">
                                <span className="text-xs text-500">
                                    <i className="pi pi-user text-xs mr-1"></i>
                                    {notification.gonderenAd}
                                </span>
                                <span className="text-xs text-500">
                                    <i className="pi pi-clock text-xs mr-1"></i>
                                    {timeAgo}
                                </span>
                            </div>
                        </div>
                    </div>
                </Card>
            </div>
        );
    };

    return (
        <div className="grid">
            <Toast ref={toast} />
            <ConfirmDialog />

            <div className="col-12">
                <Card className="shadow-3">
                    <div className="flex justify-content-between align-items-center mb-4">
                        <div>
                            <h5 className="m-0 text-900 font-bold">Bildirimler</h5>
                            <p className="m-0 mt-1 text-600">
                                Toplam {stats.total} bildirim, {stats.unread} okunmamış
                            </p>
                        </div>
                    </div>

                    {/* Filtreler */}
                    <Panel header="Filtreler" toggleable collapsed className="mb-4">
                        <div className="grid">
                            <div className="col-12 md:col-3">
                                <label htmlFor="kategori" className="block text-900 font-medium mb-2">Kategori</label>
                                <Dropdown
                                    id="kategori"
                                    value={filters.kategori}
                                    options={getKategoriOptions()}
                                    onChange={(e) => setFilters({...filters, kategori: e.value})}
                                    placeholder="Kategori seçin"
                                    className="w-full"
                                />
                            </div>
                            <div className="col-12 md:col-3">
                                <label htmlFor="tip" className="block text-900 font-medium mb-2">Tip</label>
                                <Dropdown
                                    id="tip"
                                    value={filters.tip}
                                    options={getTipOptions()}
                                    onChange={(e) => setFilters({...filters, tip: e.value})}
                                    placeholder="Tip seçin"
                                    className="w-full"
                                />
                            </div>
                            <div className="col-12 md:col-3">
                                <label htmlFor="okundu" className="block text-900 font-medium mb-2">Durum</label>
                                <Dropdown
                                    id="okundu"
                                    value={filters.okundu}
                                    options={okunduOptions}
                                    onChange={(e) => setFilters({...filters, okundu: e.value})}
                                    placeholder="Durum seçin"
                                    className="w-full"
                                />
                            </div>
                            <div className="col-12 md:col-3">
                                <label htmlFor="tarih" className="block text-900 font-medium mb-2">Tarih Aralığı</label>
                                <div className="flex gap-2">
                                    <Calendar
                                        value={filters.tarihBaslangic}
                                        onChange={(e) => setFilters({...filters, tarihBaslangic: e.value})}
                                        placeholder="Başlangıç"
                                        dateFormat="dd.mm.yy"
                                        className="w-full"
                                    />
                                    <Calendar
                                        value={filters.tarihBitis}
                                        onChange={(e) => setFilters({...filters, tarihBitis: e.value})}
                                        placeholder="Bitiş"
                                        dateFormat="dd.mm.yy"
                                        className="w-full"
                                    />
                                </div>
                            </div>
                        </div>
                    </Panel>

                    <Toolbar className="mb-4" left={leftToolbarTemplate} right={rightToolbarTemplate} />

                    {loading ? (
                        renderSkeleton()
                    ) : filteredNotifications.length === 0 ? (
                        <div className="text-center p-5">
                            <div className="mb-4">
                                <i className="pi pi-inbox" style={{ fontSize: '4rem', color: 'var(--text-color-secondary)' }}></i>
                            </div>
                            <h5 className="text-600 mb-2">Bildirim bulunamadı</h5>
                            <p className="text-500">Henüz herhangi bir bildiriminiz yok veya arama kriterlerinize uygun bildirim bulunmamaktadır.</p>
                        </div>
                    ) : viewMode === 'table' ? (
                        <DataTable
                            ref={dt}
                            value={filteredNotifications}
                            selection={selectedNotifications}
                            onSelectionChange={(e) => setSelectedNotifications(e.value)}
                            dataKey="id"
                            paginator
                            rows={10}
                            rowsPerPageOptions={[5, 10, 25, 50]}
                            className="datatable-responsive"
                            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                            currentPageReportTemplate="{first} - {last} arası, toplam {totalRecords} bildirim"
                            globalFilter={globalFilter}
                            filters={{}}
                            emptyMessage="Bildirim bulunamadı."
                            sortMode="multiple"
                            removableSort
                            size="small"
                            style={{ fontSize: '0.875rem' }}
                        >
                            <Column selectionMode="multiple" headerStyle={{ width: '3rem' }} bodyStyle={{ padding: '0.5rem' }} />
                            <Column field="baslik" header="Başlık" sortable style={{ minWidth: '200px' }} bodyStyle={{ padding: '0.5rem', fontSize: '0.85rem' }} />
                            <Column field="kategori" header="Kategori" body={categoryBodyTemplate} sortable bodyStyle={{ padding: '0.5rem' }} />
                            <Column field="gonderenAd" header="Gönderen" sortable bodyStyle={{ padding: '0.5rem', fontSize: '0.8rem' }} />
                            <Column field="olusturulmaTarihi" header="Tarih" body={dateBodyTemplate} sortable bodyStyle={{ padding: '0.5rem' }} />
                            <Column field="okundu" header="Durum" body={statusBodyTemplate} sortable bodyStyle={{ padding: '0.5rem' }} />
                            <Column header="İşlemler" body={actionBodyTemplate} exportable={false} style={{ minWidth: '100px' }} bodyStyle={{ padding: '0.5rem' }} />
                        </DataTable>
                    ) : (
                        <div className="grid">
                            {filteredNotifications.map((notification) => gridItemTemplate(notification))}
                        </div>
                    )}
                </Card>
            </div>

            {/* Bildirim Detay Dialog */}
            <Dialog
                visible={notificationDialog}
                style={{ width: '600px' }}
                header="Bildirim Detayı"
                modal
                footer={dialogFooter}
                onHide={hideDialog}
            >
                {selectedNotification && (
                    <div>
                        <div className="flex align-items-start gap-3 mb-4">
                            {notificationService.getCategoryConfig ? (
                                <Avatar
                                    icon={`pi ${notificationService.getCategoryConfig(selectedNotification.kategori).icon}`}
                                    style={{
                                        backgroundColor: notificationService.getCategoryConfig(selectedNotification.kategori).color,
                                        color: 'white'
                                    }}
                                    size="large"
                                    shape="circle"
                                />
                            ) : (
                                <Avatar
                                    icon="pi pi-info-circle"
                                    style={{ backgroundColor: '#9E9E9E', color: 'white' }}
                                    size="large"
                                    shape="circle"
                                />
                            )}
                            <div className="flex-1">
                                <h5 className="m-0 mb-2">{selectedNotification.baslik}</h5>
                                <div className="flex flex-wrap gap-2 mb-2">
                                    {notificationService.getCategoryConfig ? (
                                        <Chip
                                            label={notificationService.getCategoryConfig(selectedNotification.kategori).label}
                                            icon={`pi ${notificationService.getCategoryConfig(selectedNotification.kategori).icon}`}
                                            style={{
                                                backgroundColor: notificationService.getCategoryConfig(selectedNotification.kategori).color,
                                                color: 'white',
                                                fontSize: '0.7rem',
                                                padding: '0.2rem 0.4rem',
                                                height: '20px'
                                            }}
                                        />
                                    ) : (
                                        <Chip
                                            label={selectedNotification.kategori}
                                            icon="pi pi-info-circle"
                                            style={{
                                                backgroundColor: '#9E9E9E',
                                                color: 'white',
                                                fontSize: '0.7rem',
                                                padding: '0.2rem 0.4rem',
                                                height: '20px'
                                            }}
                                        />
                                    )}
                                    <Badge
                                        value={selectedNotification.okundu ? 'Okundu' : 'Okunmadı'}
                                        severity={selectedNotification.okundu ? 'success' : 'warning'}
                                    />
                                </div>
                            </div>
                        </div>

                        <Divider />

                        <div className="mb-4">
                            <h6 className="text-900 font-medium mb-2">Mesaj</h6>
                            <p className="m-0 line-height-3 text-600">
                                {selectedNotification.mesaj}
                            </p>
                        </div>

                        <div className="grid">
                            <div className="col-6">
                                <h6 className="text-900 font-medium mb-2">Gönderen</h6>
                                <p className="m-0 text-600">{selectedNotification.gonderenAd}</p>
                            </div>
                            <div className="col-6">
                                <h6 className="text-900 font-medium mb-2">Tarih</h6>
                                <p className="m-0 text-600">
                                    {new Date(selectedNotification.olusturulmaTarihi).toLocaleDateString('tr-TR', {
                                        day: '2-digit',
                                        month: 'long',
                                        year: 'numeric',
                                        hour: '2-digit',
                                        minute: '2-digit'
                                    })}
                                </p>
                            </div>
                            {selectedNotification.okundu && selectedNotification.okunmaTarihi && (
                                <div className="col-6">
                                    <h6 className="text-900 font-medium mb-2">Okunma Tarihi</h6>
                                    <p className="m-0 text-600">
                                        {new Date(selectedNotification.okunmaTarihi).toLocaleDateString('tr-TR', {
                                            day: '2-digit',
                                            month: 'long',
                                            year: 'numeric',
                                            hour: '2-digit',
                                            minute: '2-digit'
                                        })}
                                    </p>
                                </div>
                            )}
                        </div>

                        {selectedNotification.actionUrl && (
                            <>
                                <Divider />
                                <Message
                                    severity="info"
                                    text="Bu bildirimle ilgili işlem yapmak için 'Sayfaya Git' butonuna tıklayabilirsiniz."
                                />
                            </>
                        )}
                    </div>
                )}
            </Dialog>
        </div>
    );
};

export default Bildirimler;