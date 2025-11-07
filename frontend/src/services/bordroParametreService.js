import ApiService from './api';

class BordroParametreService {
    // Tüm bordro parametrelerini getir
    async getAll() {
        return ApiService.get('/bordroparametre');
    }

    // Belirli yıl/dönem için parametre getir
    async getByYilDonem(yil, donem) {
        return ApiService.get(`/bordroparametre/${yil}/${donem}`);
    }

    // En güncel parametreyi getir
    async getGuncel() {
        return ApiService.get('/bordroparametre/guncel');
    }

    // Yeni parametre oluştur
    async create(data) {
        return ApiService.post('/bordroparametre', data);
    }

    // Parametre güncelle
    async update(id, data) {
        return ApiService.put(`/bordroparametre/${id}`, data);
    }

    // Parametreyi pasifleştir
    async delete(id) {
        return ApiService.delete(`/bordroparametre/${id}`);
    }
}

export default new BordroParametreService();
