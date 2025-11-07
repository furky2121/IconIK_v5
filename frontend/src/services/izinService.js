import ApiService from './api';

class IzinService {
    async getAllIzinTalepleri(personelId) {
        // Artık sadece belirli bir personelin kendi taleplerini getir
        return ApiService.get(`/izintalebi?personelId=${personelId}`);
    }

    async getAllIzinTalepleriAdmin() {
        // Yönetici için tüm izin taleplerini getir
        return ApiService.get('/izintalebi/Admin');
    }

    async getBekleyenIzinTalepleri(onaylayanId) {
        // Sadece bekleyen talepleri getir - yetki kontrolü ile
        return ApiService.get(`/izintalebi/BekleyenTalepler?onaylayanId=${onaylayanId}`);
    }

    async getIzinTalebiById(id) {
        return ApiService.get(`/izintalebi/${id}`);
    }

    async getPersonelIzinOzeti(personelId) {
        return ApiService.get(`/izintalebi/PersonelOzet/${personelId}`);
    }

    async createIzinTalebi(izinTalebi) {
        return ApiService.post('/izintalebi', izinTalebi);
    }

    async updateIzinTalebi(id, izinTalebi) {
        return ApiService.put(`/izintalebi/${id}`, izinTalebi);
    }

    async onaylaIzinTalebi(id, onaylayanId, onayNotu = '') {
        return ApiService.post(`/izintalebi/Onayla/${id}`, {
            onaylayanId,
            onayNotu
        });
    }

    async reddetIzinTalebi(id, onaylayanId, onayNotu = '') {
        return ApiService.post(`/izintalebi/Reddet/${id}`, {
            onaylayanId,
            onayNotu
        });
    }

    async deleteIzinTalebi(id) {
        return ApiService.delete(`/izintalebi/${id}`);
    }

    async uploadRapor(id, file) {
        const formData = new FormData();
        formData.append('file', file);

        return ApiService.postFormData(`/izintalebi/${id}/UploadRapor`, formData);
    }

    async getIzinTakvimi(departmanId = null, kullaniciId = null) {
        let url = '/izintalebi/Takvim';
        const params = new URLSearchParams();
        
        if (departmanId) params.append('departmanId', departmanId);
        if (kullaniciId) params.append('kullaniciId', kullaniciId);
        
        if (params.toString()) {
            url += '?' + params.toString();
        }
        
        return ApiService.get(url);
    }

    async getIzinIstatistikleri(personelId = null) {
        let url = '/izintalebi/IstatistikDashboard';
        
        if (personelId) {
            url += `?personelId=${personelId}`;
        }
        
        return ApiService.get(url);
    }

    // Yardımcı metodlar
    formatTarih(tarih) {
        if (!tarih) return '';
        return new Date(tarih).toLocaleString('tr-TR', {
            day: '2-digit',
            month: '2-digit', 
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    getDurumRengi(durum) {
        switch (durum) {
            case 'Beklemede': return 'warning';
            case 'Onaylandı': return 'success';
            case 'Reddedildi': return 'danger';
            default: return 'info';
        }
    }

    getIzinTipiRengi(tip) {
        switch (tip) {
            case 'Yıllık İzin': return 'primary';
            case 'Mazeret İzni': return 'warning';
            case 'Hastalık İzni': return 'danger';
            case 'Doğum İzni': return 'success';
            default: return 'info';
        }
    }
}

export default new IzinService();