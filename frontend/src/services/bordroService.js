import ApiService from './api';

class BordroService {
    // Tüm bordroları getir (filtreleme ile)
    async getAll(filters = {}) {
        const params = new URLSearchParams();
        if (filters.yil) params.append('yil', filters.yil);
        if (filters.ay) params.append('ay', filters.ay);
        if (filters.personelId) params.append('personelId', filters.personelId);

        const queryString = params.toString();
        return ApiService.get(`/bordro${queryString ? `?${queryString}` : ''}`);
    }

    // Bordro detayını getir
    async getById(id) {
        return ApiService.get(`/bordro/${id}`);
    }

    // Tek personel için bordro hesapla
    async hesapla(data) {
        return ApiService.post('/bordro/hesapla', data);
    }

    // Toplu bordro hesapla
    async hesaplaToplu(data) {
        return ApiService.post('/bordro/hesapla-toplu', data);
    }

    // Brüt -> Net hesaplama (önizleme)
    async hesaplaBrutNet(data) {
        return ApiService.post('/bordro/hesapla-brut-net', data);
    }

    // Bordro onayla
    async onayla(id, data) {
        return ApiService.post(`/bordro/${id}/onayla`, data);
    }

    // Bordro reddet
    async reddet(id, data) {
        return ApiService.post(`/bordro/${id}/reddet`, data);
    }

    // Bordro sil
    async delete(id) {
        return ApiService.delete(`/bordro/${id}`);
    }

    // Personelin tüm bordrolarını getir
    async getPersonelBordrolar(personelId, yil = null) {
        const params = new URLSearchParams();
        if (yil) params.append('yil', yil);

        const queryString = params.toString();
        return ApiService.get(`/bordro/personel/${personelId}${queryString ? `?${queryString}` : ''}`);
    }

    // Dönem bazlı bordro özeti (dashboard için)
    async getOzet(yil, ay) {
        return ApiService.get(`/bordro/ozet?yil=${yil}&ay=${ay}`);
    }

    // Departman bazlı bordro dağılımı
    async getDepartmanDagilim(yil, ay) {
        return ApiService.get(`/bordro/departman-dagilim?yil=${yil}&ay=${ay}`);
    }
}

export default new BordroService();
