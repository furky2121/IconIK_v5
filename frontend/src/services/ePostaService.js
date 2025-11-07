import ApiService from './api';

class EPostaService {
    // ============== E-POSTA AYARLARI (SMTP Konfigürasyonu) ==============

    // Tüm SMTP ayarlarını getir
    async getSmtpAyarlari() {
        try {
            const response = await ApiService.get('/epostaayarlari');
            if (response.success) {
                return { success: true, data: response.data || [] };
            }
            return { success: false, data: [], message: response.message };
        } catch (error) {
            // console.error('Get SMTP ayarları error:', error);
            return { success: false, data: [], message: error.message };
        }
    }

    // SMTP ayarları kaydet
    async saveSmtpAyarlari(data) {
        try {
            const response = data.id
                ? await ApiService.put(`/epostaayarlari/${data.id}`, data)
                : await ApiService.post('/epostaayarlari', data);

            if (response.success) {
                return { success: true, data: response.data, message: response.message || 'SMTP ayarları başarıyla kaydedildi' };
            }
            return { success: false, message: response.message || 'SMTP ayarları kaydedilemedi' };
        } catch (error) {
            // console.error('Save SMTP ayarları error:', error);
            return { success: false, message: error.message };
        }
    }

    // SMTP bağlantı testi
    async testSmtpConnection() {
        try {
            const response = await ApiService.post('/epostaayarlari/test', {});
            if (response.success) {
                return {
                    success: true,
                    message: response.message || 'SMTP bağlantısı başarılı!',
                    details: response.details || null
                };
            }
            return {
                success: false,
                message: response.message || 'SMTP bağlantısı başarısız!',
                details: response.details || null
            };
        } catch (error) {
            // console.error('Test SMTP connection error:', error);
            return {
                success: false,
                message: error.message || 'Bağlantı hatası',
                details: error.response?.data?.details || error.response?.data?.message || JSON.stringify(error.response?.data) || null
            };
        }
    }

    // Test mail gönder (Mülakat bildirimi testi)
    async testMulakatEmail(recipientEmail, tarih) {
        try {
            const response = await ApiService.post('/epostaayarlari/test-mulakat-email', {
                recipientEmail,
                tarih
            });
            if (response.success) {
                return { success: true, message: response.message || 'Test e-postası başarıyla gönderildi!' };
            }
            return { success: false, message: response.message || 'Test e-postası gönderilemedi!' };
        } catch (error) {
            // console.error('Test mülakat email error:', error);
            return { success: false, message: error.message };
        }
    }

    // SMTP ayarlarını sil
    async deleteSmtpAyarlari(id) {
        try {
            const response = await ApiService.delete(`/epostaayarlari/${id}`);
            if (response.success) {
                return { success: true, message: response.message || 'SMTP ayarları başarıyla silindi' };
            }
            return { success: false, message: response.message };
        } catch (error) {
            // console.error('Delete SMTP ayarları error:', error);
            return { success: false, message: error.message };
        }
    }

    // ============== E-POSTA YÖNLENDİRME ==============

    // Tüm yönlendirmeleri getir
    async getYonlendirmeler() {
        try {
            const response = await ApiService.get('/epostayonlendirme');
            if (response.success) {
                return { success: true, data: response.data || [] };
            }
            return { success: false, data: [], message: response.message };
        } catch (error) {
            // console.error('Get yönlendirmeler error:', error);
            return { success: false, data: [], message: error.message };
        }
    }

    // Yönlendirme kaydet
    async saveYonlendirme(data) {
        try {
            const response = data.id
                ? await ApiService.put(`/epostayonlendirme/${data.id}`, data)
                : await ApiService.post('/epostayonlendirme', data);

            if (response.success) {
                return { success: true, data: response.data, message: response.message || 'Yönlendirme başarıyla kaydedildi' };
            }
            return { success: false, message: response.message || 'Yönlendirme kaydedilemedi' };
        } catch (error) {
            // console.error('Save yönlendirme error:', error);
            return { success: false, message: error.message };
        }
    }

    // Yönlendirmeyi sil
    async deleteYonlendirme(id) {
        try {
            const response = await ApiService.delete(`/epostayonlendirme/${id}`);
            if (response.success) {
                return { success: true, message: response.message || 'Yönlendirme başarıyla silindi' };
            }
            return { success: false, message: response.message };
        } catch (error) {
            // console.error('Delete yönlendirme error:', error);
            return { success: false, message: error.message };
        }
    }

    // Yönlendirme türlerini getir
    async getYonlendirmeTurleri() {
        try {
            const response = await ApiService.get('/epostayonlendirme/turler');
            if (response.success) {
                return { success: true, data: response.data || [] };
            }
            return { success: false, data: [], message: response.message };
        } catch (error) {
            // console.error('Get yönlendirme türleri error:', error);
            return { success: false, data: [], message: error.message };
        }
    }
}

export default new EPostaService();
