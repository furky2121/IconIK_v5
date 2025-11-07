import ApiService from './api';

class KesintiTanimiService {
    // Tüm kesinti tanımlarını getir
    async getAll() {
        return ApiService.get('/kesintitanimlari');
    }

    // Sadece aktif kesinti tanımlarını getir (dropdown için)
    async getAktif() {
        return ApiService.get('/kesintitanimlari/aktif');
    }

    // Belirli bir kesinti tanımını getir
    async getById(id) {
        return ApiService.get(`/kesintitanimlari/${id}`);
    }

    // Kesinti türüne göre tanımları getir
    async getByTur(kesintiTuru) {
        return ApiService.get(`/kesintitanimlari/tur/${kesintiTuru}`);
    }

    // Kesinti türlerini grupla ve say
    async getTurOzeti() {
        return ApiService.get('/kesintitanimlari/tur-ozeti');
    }

    // Taksitlendirilebilir kesintileri getir
    async getTaksitlenebilir() {
        return ApiService.get('/kesintitanimlari/taksitlenebilir');
    }

    // Otomatik hesaplanan kesintileri getir
    async getOtomatik() {
        return ApiService.get('/kesintitanimlari/otomatik');
    }

    // Yeni kesinti tanımı oluştur
    async create(data) {
        return ApiService.post('/kesintitanimlari', data);
    }

    // Kesinti tanımını güncelle
    async update(id, data) {
        return ApiService.put(`/kesintitanimlari/${id}`, data);
    }

    // Kesinti tanımını pasifleştir veya sil
    async delete(id) {
        return ApiService.delete(`/kesintitanimlari/${id}`);
    }
}

export default new KesintiTanimiService();
