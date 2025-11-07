import api from './api';
import authService from './authService';

class YetkiService {
    // Get all screen permissions
    async getEkranYetkileri() {
        try {
            const response = await api.get('/Yetki/EkranYetkileri');
            return response; // api.js already returns the data
        } catch (error) {
            // console.error('Error fetching screen permissions:', error);
            throw error;
        }
    }

    // Get all kademe permissions
    async getKademeYetkileri() {
        try {
            const response = await api.get('/Yetki/KademeYetkileri');
            return response; // api.js already returns the data
        } catch (error) {
            // console.error('Error fetching kademe permissions:', error);
            throw error;
        }
    }

    // Get permissions for specific kademe
    async getKademeYetkileriByKademe(kademeId) {
        try {
            const response = await api.get(`/Yetki/KademeYetkileri/${kademeId}`);
            return response; // api.js already returns the data
        } catch (error) {
            // console.error('Error fetching kademe permissions:', error);
            throw error;
        }
    }

    // Create new screen permission
    async createEkranYetkisi(ekranYetkisi) {
        try {
            const response = await api.post('/Yetki/EkranYetkisi', ekranYetkisi);
            return response.data;
        } catch (error) {
            // console.error('Error creating screen permission:', error);
            throw error;
        }
    }

    // Create new kademe permission
    async createKademeYetkisi(kademeYetkisi) {
        try {
            const response = await api.post('/Yetki/KademeYetkisi', kademeYetkisi);
            return response.data;
        } catch (error) {
            // console.error('Error creating kademe permission:', error);
            throw error;
        }
    }

    // Update kademe permission
    async updateKademeYetkisi(id, kademeYetkisi) {
        try {
            const response = await api.put(`/Yetki/KademeYetkisi/${id}`, kademeYetkisi);
            return response.data;
        } catch (error) {
            // console.error('Error updating kademe permission:', error);
            throw error;
        }
    }

    // Delete kademe permission
    async deleteKademeYetkisi(id) {
        try {
            const response = await api.delete(`/Yetki/KademeYetkisi/${id}`);
            return response.data;
        } catch (error) {
            // console.error('Error deleting kademe permission:', error);
            throw error;
        }
    }

    // Create default screen permissions
    async createDefaultEkranYetkileri() {
        try {
            const response = await api.post('/Yetki/DefaultEkranYetkileri');
            return response.data;
        } catch (error) {
            // console.error('Error creating default screen permissions:', error);
            throw error;
        }
    }

    // Create default kademe permissions
    async createDefaultKademeYetkileri() {
        try {
            const response = await api.post('/Yetki/DefaultKademeYetkileri');
            return response.data;
        } catch (error) {
            // console.error('Error creating default kademe permissions:', error);
            throw error;
        }
    }

    // Check if current user has permission for a screen
    hasScreenPermission(screenCode, permissionType = 'read') {
        try {
            const user = authService.getUser();

            if (!user) {
                return false;
            }

            // Check for Genel Müdür by username first (most reliable)
            // Backend response has a nested structure: user.kullanici.kullaniciAdi
            const kullaniciAdi = user.kullaniciAdi || user.kullanici?.kullaniciAdi;
            if (kullaniciAdi === 'ahmet.yilmaz') {
                return true;
            }

            // Check kademe structure - handle both user.personel and user.kullanici.personel
            const personel = user.personel || user.kullanici?.personel;
            if (!personel || !personel.pozisyon || !personel.pozisyon.kademe) {
                // If structure incomplete, check if it's still Genel Müdür by any chance
                return false;
            }

            const userKademe = personel.pozisyon.kademe.seviye;

            // Genel Müdür (seviye 1) has all permissions
            if (userKademe === 1) {
                return true;
            }

            // Get cached permissions from localStorage
            const permissions = this.getCachedPermissions();
            
            if (!permissions) {
                // If no cached permissions, allow basic screens but restrict sensitive ones
                const restrictedScreens = ['bordrolar', 'departmanlar', 'kademeler', 'pozisyonlar'];
                if (restrictedScreens.includes(screenCode)) {
                    return false;
                }
                return permissionType === 'read'; // Allow read access to other screens
            }

            const userPermissions = permissions.filter(p => p.kademeSeviye === userKademe);
            const screenPermission = userPermissions.find(p => p.ekranKodu === screenCode);

            if (!screenPermission) {
                return false;
            }

            switch (permissionType.toLowerCase()) {
                case 'read':
                case 'okuma':
                    return screenPermission.okumaYetkisi;
                case 'write':
                case 'yazma':
                    return screenPermission.yazmaYetkisi;
                case 'delete':
                case 'silme':
                    return screenPermission.silmeYetkisi;
                case 'update':
                case 'guncelleme':
                    return screenPermission.guncellemeYetkisi;
                default:
                    return screenPermission.okumaYetkisi;
            }
        } catch (error) {
            return false;
        }
    }

    // Cache permissions in localStorage
    setCachedPermissions(permissions) {
        try {
            localStorage.setItem('user_permissions', JSON.stringify(permissions));
        } catch (error) {
            // Silent fail
        }
    }

    // Get cached permissions from localStorage
    getCachedPermissions() {
        try {
            const permissions = localStorage.getItem('user_permissions');
            return permissions ? JSON.parse(permissions) : null;
        } catch (error) {
            return null;
        }
    }

    // Load and cache permissions for current user
    async loadUserPermissions() {
        try {
            const user = authService.getUser();
            if (!user) {
                return;
            }

            // Check multiple possible paths for kademe ID - handle both user.personel and user.kullanici.personel
            let kademeId = null;
            const personel = user.personel || user.kullanici?.personel;

            if (personel?.pozisyon?.kademe?.id) {
                kademeId = personel.pozisyon.kademe.id;
            } else if (personel?.pozisyon?.kademeId) {
                kademeId = personel.pozisyon.kademeId;
            } else if (personel?.kademeId) {
                kademeId = personel.kademeId;
            }

            if (!kademeId) {
                // Return empty permissions but don't fail
                this.setCachedPermissions([]);
                return [];
            }

            const permissions = await this.getKademeYetkileriByKademe(kademeId);
            this.setCachedPermissions(permissions);

            return permissions;
        } catch (error) {
            // Set empty permissions on error to prevent further issues
            this.setCachedPermissions([]);
            throw error;
        }
    }

    // Clear cached permissions
    clearCachedPermissions() {
        localStorage.removeItem('user_permissions');
    }

    // Get filtered menu items based on permissions
    getFilteredMenuItems(menuItems) {
        return menuItems.filter(item => {
            if (!item.screenCode) {
                return true; // If no screenCode, allow by default
            }
            return this.hasScreenPermission(item.screenCode, 'read');
        });
    }
}

export default new YetkiService();