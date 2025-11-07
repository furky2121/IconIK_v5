import ApiService from './api';

class FirmaAyarlariService {
    async get() {
        return ApiService.get('/firmaayarlari');
    }

    async update(firmaAyarlari) {
        return ApiService.put('/firmaayarlari', firmaAyarlari);
    }

    async uploadLogo(file) {
        const formData = new FormData();
        formData.append('file', file);
        return ApiService.postFormData('/firmaayarlari/UploadLogo', formData);
    }

    async deleteLogo() {
        return ApiService.delete('/firmaayarlari/DeleteLogo');
    }
}

export default new FirmaAyarlariService();
