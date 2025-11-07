import ApiService from './api';

class KVKKService {
    async getAll() {
        return await ApiService.get('/kvkkizinmetni');
    }

    async getAktif() {
        return await ApiService.get('/kvkkizinmetni/aktif');
    }

    async getById(id) {
        return await ApiService.get(`/kvkkizinmetni/${id}`);
    }

    async create(data) {
        return await ApiService.post('/kvkkizinmetni', data);
    }

    async update(id, data) {
        return await ApiService.put(`/kvkkizinmetni/${id}`, data);
    }

    async delete(id) {
        return await ApiService.delete(`/kvkkizinmetni/${id}`);
    }

    async onayKaydet(kullaniciId, onay) {
        return await ApiService.post('/kvkkizinmetni/onaykaydet', {
            kullaniciId,
            onay
        });
    }

    async getOnayDurumu(kullaniciId) {
        return await ApiService.get(`/kvkkizinmetni/onaydurumu/${kullaniciId}`);
    }
}

export default new KVKKService();
