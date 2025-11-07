import ApiService from './api';

class PuantajService {
    // Tüm puantajları getir (filtreleme ile)
    async getAll(filters = {}) {
        const params = new URLSearchParams();
        if (filters.yil) params.append('yil', filters.yil);
        if (filters.ay) params.append('ay', filters.ay);
        if (filters.personelId) params.append('personelId', filters.personelId);

        const queryString = params.toString();
        return ApiService.get(`/puantaj${queryString ? `?${queryString}` : ''}`);
    }

    // Puantaj detayını getir
    async getById(id) {
        return ApiService.get(`/puantaj/${id}`);
    }

    // Yeni puantaj oluştur
    async create(data) {
        return ApiService.post('/puantaj', data);
    }

    // Puantaj güncelle
    async update(id, data) {
        return ApiService.put(`/puantaj/${id}`, data);
    }

    // Puantaj onayla
    async onayla(id, data) {
        return ApiService.post(`/puantaj/${id}/onayla`, data);
    }

    // Puantaj reddet
    async reddet(id, data) {
        return ApiService.post(`/puantaj/${id}/reddet`, data);
    }

    // Puantaj sil
    async delete(id) {
        return ApiService.delete(`/puantaj/${id}`);
    }
}

export default new PuantajService();
