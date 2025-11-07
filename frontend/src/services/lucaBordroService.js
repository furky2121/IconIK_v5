import ApiService from './api';

class LucaBordroService {
    // Kullanıcının kendi bordrolarını getir
    async getBenimBordrolarim() {
        return ApiService.get('/lucabordro/benim-bordrolarim');
    }

    // Bordro detayını getir
    async getById(id) {
        return ApiService.get(`/lucabordro/${id}`);
    }

    // Luca'dan senkronize et (yöneticiler için)
    async senkronize() {
        return ApiService.post('/lucabordro/senkronize');
    }

    // Dosyadan bordro yükle (yöneticiler için)
    async dosyaYukle(formData) {
        return ApiService.post('/lucabordro/dosya-yukle', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
    }

    // OTP kodu gönder
    async otpGonder(bordroId) {
        return ApiService.post(`/lucabordro/${bordroId}/otp-gonder`);
    }

    // OTP doğrula ve mail'e gönder
    async otpDogrulaVeGonder(bordroId, otpKodu) {
        return ApiService.post(`/lucabordro/${bordroId}/otp-dogrula-ve-gonder`, { otpKodu });
    }
}

export default new LucaBordroService();
