import ApiService from './api';

class LucaBordroAyarlariService {
    // Tüm ayarları getir
    async getAll() {
        return ApiService.get('/lucabordroayarlari');
    }

    // Ayar detayını getir
    async getById(id) {
        return ApiService.get(`/lucabordroayarlari/${id}`);
    }

    // Aktif ayarı getir
    async getAktif() {
        return ApiService.get('/lucabordroayarlari/aktif');
    }

    // Yeni ayar oluştur
    async create(data) {
        return ApiService.post('/lucabordroayarlari', data);
    }

    // Ayar güncelle
    async update(id, data) {
        return ApiService.put(`/lucabordroayarlari/${id}`, data);
    }

    // Ayar sil
    async delete(id) {
        return ApiService.delete(`/lucabordroayarlari/${id}`);
    }

    // Bağlantı testi
    async testBaglanti(id) {
        return ApiService.post(`/lucabordroayarlari/${id}/test-baglanti`);
    }
}

export default new LucaBordroAyarlariService();
