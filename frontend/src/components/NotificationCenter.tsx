import React, { useState, useEffect, useRef } from 'react';
import { Button } from 'primereact/button';
import { Badge } from 'primereact/badge';
import { OverlayPanel } from 'primereact/overlaypanel';
import { Avatar } from 'primereact/avatar';
import { Divider } from 'primereact/divider';
import { ScrollPanel } from 'primereact/scrollpanel';
import { Toast } from 'primereact/toast';
import { confirmDialog, ConfirmDialog } from 'primereact/confirmdialog';
import notificationService from '../services/notificationService';
import authService from '../services/authService';

interface Notification {
    id: string;
    aliciId: number;
    baslik: string;
    mesaj: string;
    kategori: string;
    tip: string;
    olusturulmaTarihi: string;
    okundu: boolean;
    okunmaTarihi: string | null;
    gonderenAd: string;
    actionUrl?: string;
}

const NotificationCenter: React.FC = () => {
    const [notifications, setNotifications] = useState<Notification[]>([]);
    const [unreadCount, setUnreadCount] = useState<number>(0);
    const [loading, setLoading] = useState<boolean>(false);
    const [currentUser, setCurrentUser] = useState<any>(null);

    const overlayRef = useRef<OverlayPanel>(null);
    const toast = useRef<Toast>(null);

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

    const loadNotifications = async (personelId: number) => {
        setLoading(true);
        try {
            const [allResult, unreadResult] = await Promise.all([
                notificationService.getAllNotifications(personelId),
                notificationService.getUnreadCount(personelId)
            ]);

            if (allResult.success) {
                setNotifications(allResult.data.slice(0, 10)); // Son 10 bildirim
            }
            setUnreadCount(unreadResult);
        } catch (error) {
            // console.error('Load notifications error:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleNotificationClick = async (notification: Notification) => {
        // Okunmamışsa okundu işaretle
        if (!notification.okundu) {
            await notificationService.markAsRead(notification.id);
            await loadNotifications(currentUser.personelId);
        }

        // Action URL varsa yönlendir
        if (notification.actionUrl) {
            window.location.href = notification.actionUrl;
        }

        // Overlay'i kapat
        overlayRef.current?.hide();
    };

    const markAllAsRead = async () => {
        if (currentUser?.personelId) {
            const result = await notificationService.markAllAsRead(currentUser.personelId);
            if (result.success) {
                await loadNotifications(currentUser.personelId);
                toast.current?.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Tüm bildirimler okundu olarak işaretlendi',
                    life: 3000
                });
            }
        }
    };

    const clearAllNotifications = () => {
        confirmDialog({
            message: 'Tüm bildirimleri silmek istediğinizden emin misiniz?',
            header: 'Bildirimleri Temizle',
            icon: 'pi pi-exclamation-triangle',
            accept: async () => {
                // Tüm bildirimleri sil (mock implementation)
                notifications.forEach(async (notification) => {
                    await notificationService.deleteNotification(notification.id);
                });

                await loadNotifications(currentUser.personelId);
                toast.current?.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Tüm bildirimler temizlendi',
                    life: 3000
                });
            }
        });
    };

    const getCategoryIcon = (kategori: string) => {
        const config = notificationService.getCategoryConfig(kategori);
        return config.icon;
    };

    const getCategoryColor = (kategori: string) => {
        const config = notificationService.getCategoryConfig(kategori);
        return config.color;
    };

    const formatTimeAgo = (dateString: string) => {
        return notificationService.formatTimeAgo(dateString);
    };

    const toggleOverlay = (event: React.MouseEvent) => {
        overlayRef.current?.toggle(event);
    };

    const goToAllNotifications = () => {
        overlayRef.current?.hide();
        window.location.href = '/bildirimler';
    };

    return (
        <>
            <Toast ref={toast} position="top-right" />
            <ConfirmDialog />

            <div className="notification-center" style={{ position: 'relative', marginRight: '1rem' }}>
                <Button
                    icon="pi pi-bell"
                    className="p-button-rounded p-button-text"
                    onClick={toggleOverlay}
                    style={{
                        color: 'var(--text-color)',
                        position: 'relative'
                    }}
                    tooltip="Bildirimler"
                    tooltipOptions={{ position: 'bottom' }}
                />

                {unreadCount > 0 && (
                    <Badge
                        value={unreadCount > 99 ? '99+' : unreadCount.toString()}
                        severity="danger"
                        style={{
                            position: 'absolute',
                            top: '-5px',
                            right: '-5px',
                            minWidth: '18px',
                            height: '18px',
                            fontSize: '10px',
                            borderRadius: '9px'
                        }}
                    />
                )}

                <OverlayPanel
                    ref={overlayRef}
                    showCloseIcon
                    style={{ width: '420px', maxHeight: '600px' }}
                    className="notification-overlay"
                >
                    <div className="notification-header">
                        <div className="flex justify-content-between align-items-center mb-3">
                            <div>
                                <h5 className="m-0 font-bold text-900">Bildirimler</h5>
                                {unreadCount > 0 && (
                                    <span className="text-sm text-500 mt-1">
                                        {unreadCount} okunmamış bildirim
                                    </span>
                                )}
                            </div>
                            <div className="flex gap-2">
                                {unreadCount > 0 && (
                                    <Button
                                        icon="pi pi-check-circle"
                                        className="p-button-text p-button-sm p-button-rounded"
                                        onClick={markAllAsRead}
                                        tooltip="Tümünü Okundu İşaretle"
                                        tooltipOptions={{ position: 'bottom' }}
                                        style={{ color: 'var(--green-500)' }}
                                    />
                                )}
                                <Button
                                    icon="pi pi-trash"
                                    className="p-button-text p-button-sm p-button-rounded"
                                    onClick={clearAllNotifications}
                                    tooltip="Tümünü Temizle"
                                    tooltipOptions={{ position: 'bottom' }}
                                    style={{ color: 'var(--red-500)' }}
                                />
                            </div>
                        </div>
                        <Divider className="my-2" />
                    </div>

                    <ScrollPanel style={{ width: '100%', height: '400px' }}>
                        {loading ? (
                            <div className="text-center p-5">
                                <i className="pi pi-spinner pi-spin text-primary" style={{ fontSize: '3rem' }}></i>
                                <p className="mt-3 text-600 font-medium">Bildirimler yükleniyor...</p>
                            </div>
                        ) : notifications.length === 0 ? (
                            <div className="text-center p-5">
                                <div className="mb-3">
                                    <i className="pi pi-bell-slash" style={{ fontSize: '4rem', color: 'var(--text-color-secondary)' }}></i>
                                </div>
                                <h6 className="text-700 mb-2">Henüz bildiriminiz yok</h6>
                                <p className="text-500 text-sm m-0">Yeni bildirimler burada görünecektir</p>
                            </div>
                        ) : (
                            <div className="notification-list">
                                {notifications.map((notification, index) => {
                                    const categoryColor = getCategoryColor(notification.kategori);
                                    return (
                                        <div
                                            key={notification.id}
                                            className="notification-item cursor-pointer border-round transition-all transition-duration-200"
                                            onClick={() => handleNotificationClick(notification)}
                                            style={{
                                                padding: '0.875rem',
                                                backgroundColor: !notification.okundu ? 'var(--surface-50)' : 'white',
                                                borderLeft: `4px solid ${!notification.okundu ? categoryColor : 'transparent'}`,
                                                marginBottom: index < notifications.length - 1 ? '0.75rem' : 0,
                                                transition: 'all 0.2s ease'
                                            }}
                                            onMouseEnter={(e) => {
                                                e.currentTarget.style.backgroundColor = 'var(--surface-100)';
                                                e.currentTarget.style.boxShadow = '0 2px 12px rgba(0,0,0,0.15)';
                                            }}
                                            onMouseLeave={(e) => {
                                                e.currentTarget.style.backgroundColor = !notification.okundu ? 'var(--surface-50)' : 'white';
                                                e.currentTarget.style.boxShadow = 'none';
                                            }}
                                        >
                                            <div className="flex align-items-start gap-3">
                                                <Avatar
                                                    icon={`pi ${getCategoryIcon(notification.kategori)}`}
                                                    style={{
                                                        backgroundColor: categoryColor,
                                                        color: 'white',
                                                        minWidth: '42px',
                                                        width: '42px',
                                                        height: '42px'
                                                    }}
                                                    size="normal"
                                                    shape="circle"
                                                />

                                                <div className="flex-1 min-w-0">
                                                    <div className="flex justify-content-between align-items-start mb-1">
                                                        <h6 className={`m-0 text-sm ${!notification.okundu ? 'font-bold text-900' : 'font-medium text-700'}`}>
                                                            {notification.baslik}
                                                        </h6>
                                                        {!notification.okundu && (
                                                            <div
                                                                className="border-circle"
                                                                style={{
                                                                    backgroundColor: categoryColor,
                                                                    width: '10px',
                                                                    height: '10px',
                                                                    marginLeft: '0.5rem',
                                                                    marginTop: '0.25rem',
                                                                    flexShrink: 0
                                                                }}
                                                            />
                                                        )}
                                                    </div>

                                                    <p className="m-0 mt-1 text-sm text-600 line-height-3">
                                                        {notification.mesaj.length > 85
                                                            ? notification.mesaj.substring(0, 85) + '...'
                                                            : notification.mesaj
                                                        }
                                                    </p>

                                                    <div className="flex justify-content-between align-items-center mt-2">
                                                        <span className="text-xs text-500 flex align-items-center gap-1">
                                                            <i className="pi pi-user" style={{ fontSize: '0.7rem' }}></i>
                                                            {notification.gonderenAd}
                                                        </span>
                                                        <span className="text-xs text-500 flex align-items-center gap-1">
                                                            <i className="pi pi-clock" style={{ fontSize: '0.7rem' }}></i>
                                                            {formatTimeAgo(notification.olusturulmaTarihi)}
                                                        </span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </ScrollPanel>

                    {notifications.length > 0 && (
                        <>
                            <Divider className="my-2" />
                            <div className="text-center">
                                <Button
                                    label="Tüm Bildirimleri Görüntüle"
                                    icon="pi pi-external-link"
                                    className="p-button-text p-button-sm"
                                    onClick={goToAllNotifications}
                                />
                            </div>
                        </>
                    )}
                </OverlayPanel>
            </div>
        </>
    );
};

export default NotificationCenter;