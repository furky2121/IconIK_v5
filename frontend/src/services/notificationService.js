import ApiService from './api';

class NotificationService {

    // Notification kategorileri
    static CATEGORIES = {
        IZIN: 'izin',
        EGITIM: 'egitim',
        DOGUM_GUNU: 'dogum_gunu',
        SISTEM: 'sistem',
        AVANS: 'avans',
        ISTIFA: 'istifa',
        MASRAF: 'masraf',
        DUYURU: 'duyuru',
        ANKET: 'anket'
    };

    // Notification tipleri
    static TYPES = {
        INFO: 'info',
        SUCCESS: 'success',
        WARNING: 'warning',
        ERROR: 'error'
    };

    constructor() {
        // Sadece API üzerinden çalışacak
    }

    // Tüm bildirimleri getir
    async getAllNotifications(personelId) {
        try {
            const response = await ApiService.get(`/bildirim/personel/${personelId}`);
            if (response.success) {
                return { success: true, data: response.data || [] };
            }
            return { success: false, data: [], message: response.message };
        } catch (error) {
            // console.error('Get notifications error:', error);
            return { success: false, data: [], message: error.message };
        }
    }

    // Okunmamış bildirimleri getir
    async getUnreadNotifications(personelId) {
        try {
            const response = await ApiService.get(`/bildirim/personel/${personelId}/okunmamis`);
            if (response.success) {
                return { success: true, data: response.data || [] };
            }
            return { success: false, data: [], message: response.message };
        } catch (error) {
            // console.error('Get unread notifications error:', error);
            return { success: false, data: [], message: error.message };
        }
    }

    // Okunmamış bildirim sayısını getir
    async getUnreadCount(personelId) {
        try {
            const response = await ApiService.get(`/bildirim/personel/${personelId}/okunmamis-sayisi`);
            if (response.success) {
                return response.data || 0;
            }
            return 0;
        } catch (error) {
            // console.error('Get unread count error:', error);
            return 0;
        }
    }

    // Bildirimi okundu olarak işaretle
    async markAsRead(notificationId) {
        try {
            const response = await ApiService.put(`/bildirim/${notificationId}/read`, {});
            return response;
        } catch (error) {
            // console.error('Mark as read error:', error);
            return { success: false, message: error.message };
        }
    }

    // Tüm bildirimleri okundu işaretle
    async markAllAsRead(personelId) {
        try {
            const response = await ApiService.post(`/bildirim/personel/${personelId}/read-all`, {});
            return response;
        } catch (error) {
            // console.error('Mark all as read error:', error);
            return { success: false, message: error.message };
        }
    }

    // Bildirimi sil
    async deleteNotification(notificationId) {
        try {
            const response = await ApiService.delete(`/bildirim/${notificationId}`);
            return response;
        } catch (error) {
            // console.error('Delete notification error:', error);
            return { success: false, message: error.message };
        }
    }

    // Yeni bildirim ekle
    async addNotification(notification) {
        try {
            const response = await ApiService.post('/bildirim', notification);
            return response;
        } catch (error) {
            // console.error('Add notification error:', error);
            return { success: false, message: error.message };
        }
    }

    // Kategori bazlı filtreleme
    async getNotificationsByCategory(personelId, category) {
        try {
            const all = await this.getAllNotifications(personelId);
            const filtered = all.data.filter(n => n.kategori === category);
            return { ...all, data: filtered };
        } catch (error) {
            // console.error('Get notifications by category error:', error);
            return { success: false, data: [], message: error.message };
        }
    }

    // Zaman formatı helper
    formatTimeAgo(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffInSeconds = Math.floor((now - date) / 1000);

        if (diffInSeconds < 60) {
            return 'Az önce';
        } else if (diffInSeconds < 3600) {
            const minutes = Math.floor(diffInSeconds / 60);
            return `${minutes} dakika önce`;
        } else if (diffInSeconds < 86400) {
            const hours = Math.floor(diffInSeconds / 3600);
            return `${hours} saat önce`;
        } else {
            const days = Math.floor(diffInSeconds / 86400);
            return `${days} gün önce`;
        }
    }

    // Kategori ikon ve renk mappingi
    getCategoryConfig(category) {
        const configs = {
            [NotificationService.CATEGORIES.IZIN]: {
                icon: 'pi-calendar',
                color: '#2196F3',
                label: 'İzin'
            },
            [NotificationService.CATEGORIES.EGITIM]: {
                icon: 'pi-book',
                color: '#FF9800',
                label: 'Eğitim'
            },
            [NotificationService.CATEGORIES.DOGUM_GUNU]: {
                icon: 'pi-heart',
                color: '#E91E63',
                label: 'Doğum Günü'
            },
            [NotificationService.CATEGORIES.SISTEM]: {
                icon: 'pi-cog',
                color: '#607D8B',
                label: 'Sistem'
            },
            [NotificationService.CATEGORIES.AVANS]: {
                icon: 'pi-dollar',
                color: '#4CAF50',
                label: 'Avans'
            },
            [NotificationService.CATEGORIES.ISTIFA]: {
                icon: 'pi-sign-out',
                color: '#F44336',
                label: 'İstifa'
            },
            [NotificationService.CATEGORIES.MASRAF]: {
                icon: 'pi-credit-card',
                color: '#9C27B0',
                label: 'Masraf'
            },
            [NotificationService.CATEGORIES.DUYURU]: {
                icon: 'pi-megaphone',
                color: '#FF5722',
                label: 'Duyuru'
            },
            [NotificationService.CATEGORIES.ANKET]: {
                icon: 'pi-chart-bar',
                color: '#00BCD4',
                label: 'Anket'
            }
        };

        return configs[category] || {
            icon: 'pi-info-circle',
            color: '#9E9E9E',
            label: 'Bilgi'
        };
    }
}

export { NotificationService };
export default new NotificationService();
