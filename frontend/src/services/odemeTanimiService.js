import ApiService from './api';

class OdemeTanimiService {
    // Tüm ödeme tanımlarını getir
    async getAll() {
        return ApiService.get('/odemetanimlari');
    }

    // Sadece aktif ödeme tanımlarını getir (dropdown için)
    async getAktif() {
        return ApiService.get('/odemetanimlari/aktif');
    }

    // Belirli bir ödeme tanımını getir
    async getById(id) {
        return ApiService.get(`/odemetanimlari/${id}`);
    }

    // Ödeme türüne göre tanımları getir
    async getByTur(odemeTuru) {
        return ApiService.get(`/odemetanimlari/tur/${odemeTuru}`);
    }

    // Ödeme türlerini grupla ve say
    async getTurOzeti() {
        return ApiService.get('/odemetanimlari/tur-ozeti');
    }

    // Yeni ödeme tanımı oluştur
    async create(data) {
        return ApiService.post('/odemetanimlari', data);
    }

    // Ödeme tanımını güncelle
    async update(id, data) {
        return ApiService.put(`/odemetanimlari/${id}`, data);
    }

    // Ödeme tanımını pasifleştir veya sil
    async delete(id) {
        return ApiService.delete(`/odemetanimlari/${id}`);
    }
}

export default new OdemeTanimiService();
