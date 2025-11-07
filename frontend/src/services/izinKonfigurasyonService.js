import ApiService from './api';

class IzinKonfigurasyonService {

    // İzin Tipleri CRUD işlemleri
    async getAllIzinTipleri() {
        return await ApiService.get('/izintipi');
    }

    async getAktifIzinTipleri() {
        return await ApiService.get('/izintipi/aktif');
    }

    async getIzinTipiById(id) {
        return await ApiService.get(`/izintipi/${id}`);
    }

    async createIzinTipi(izinTipi) {
        return await ApiService.post('/izintipi', izinTipi);
    }

    async updateIzinTipi(id, izinTipi) {
        return await ApiService.put(`/izintipi/${id}`, izinTipi);
    }

    async deleteIzinTipi(id) {
        return await ApiService.delete(`/izintipi/${id}`);
    }

    // Yıllık İzin Kuralları CRUD işlemleri
    async getAllYillikIzinKurallari() {
        return await ApiService.get('/yillikizinkurallari');
    }

    async getAktifYillikIzinKurallari() {
        return await ApiService.get('/yillikizinkurallari/aktif');
    }

    async getYillikIzinKuraliById(id) {
        return await ApiService.get(`/yillikizinkurallari/${id}`);
    }

    async createYillikIzinKurali(kural) {
        return await ApiService.post('/yillikizinkurallari', kural);
    }

    async updateYillikIzinKurali(id, kural) {
        return await ApiService.put(`/yillikizinkurallari/${id}`, kural);
    }

    async deleteYillikIzinKurali(id) {
        return await ApiService.delete(`/yillikizinkurallari/${id}`);
    }

    // İzin hesaplama metodları
    async calculateYillikIzinHakki(personelId, yil) {
        return await ApiService.get(`/izinhesaplama/yillik?personelId=${personelId}&yil=${yil}`);
    }

    async getPersonelKidemYili(personelId) {
        return await ApiService.get(`/izinhesaplama/kidem?personelId=${personelId}`);
    }

    // Cinsiyet bazlı izin tipleri
    async getIzinTipleriByGender(cinsiyet) {
        return await ApiService.get(`/izintipi/aktif?cinsiyet=${cinsiyet}`);
    }

    // Konfigürasyon ayarları
    async getIzinKonfigurasyonlari() {
        return await ApiService.get('/izinkonfigurasyonlari/ayarlar');
    }

    async updateIzinKonfigurasyonlari(ayarlar) {
        return await ApiService.put('/izinkonfigurasyonlari/ayarlar', ayarlar);
    }

    // Yasal izin hesaplamaları
    async getYasalIzinDurumlari() {
        return await ApiService.get('/izinkonfigurasyonlari/yasal-durumlar');
    }

    // İzin tipine göre maksimum gün sınırları
    async getIzinTipiSinirlari(izinTipiId, personelId) {
        return await ApiService.get(`/izinkonfigurasyonlari/sinirlar?izinTipiId=${izinTipiId}&personelId=${personelId}`);
    }

    // Rapor zorunluluğu kontrolü
    async checkRaporZorunlulugu(izinTipiId) {
        return await ApiService.get(`/izinkonfigurasyonlari/rapor-kontrol?izinTipiId=${izinTipiId}`);
    }

    // Mock metodları - API hazır olana kadar
    async getMockIzinTipleri() {
        return new Promise((resolve) => {
            setTimeout(() => {
                resolve({
                    success: true,
                    data: [
                        {
                            id: 1,
                            izinTipiAdi: 'Yıllık İzin',
                            standartGunSayisi: 14,
                            maksimumGunSayisi: 26,
                            cinsiyetKisiti: null,
                            raporGerekli: false,
                            aciklama: 'Kıdem yılına göre hesaplanır',
                            aktif: true
                        },
                        {
                            id: 2,
                            izinTipiAdi: 'Doğum İzni',
                            standartGunSayisi: 112,
                            maksimumGunSayisi: 112,
                            cinsiyetKisiti: 'K',
                            raporGerekli: true,
                            aciklama: '16 hafta doğum izni',
                            aktif: true
                        },
                        {
                            id: 3,
                            izinTipiAdi: 'Hastalık İzni',
                            standartGunSayisi: 1,
                            maksimumGunSayisi: null,
                            cinsiyetKisiti: null,
                            raporGerekli: true,
                            aciklama: 'Hekim raporu ile',
                            aktif: true
                        },
                        {
                            id: 4,
                            izinTipiAdi: 'Evlilik İzni',
                            standartGunSayisi: 3,
                            maksimumGunSayisi: 3,
                            cinsiyetKisiti: null,
                            raporGerekli: false,
                            aciklama: 'Evlilik belgesi ile',
                            aktif: true
                        },
                        {
                            id: 5,
                            izinTipiAdi: 'Ölüm İzni',
                            standartGunSayisi: 3,
                            maksimumGunSayisi: 7,
                            cinsiyetKisiti: null,
                            raporGerekli: false,
                            aciklama: 'Yakınlık derecesine göre',
                            aktif: true
                        }
                    ]
                });
            }, 500);
        });
    }

    async getMockYillikIzinKurallari() {
        return new Promise((resolve) => {
            setTimeout(() => {
                resolve({
                    success: true,
                    data: [
                        {
                            id: 1,
                            minKidemYili: 1,
                            maxKidemYili: 5,
                            izinGunSayisi: 14,
                            aciklama: '1-5 yıl arası çalışanlar',
                            aktif: true
                        },
                        {
                            id: 2,
                            minKidemYili: 5,
                            maxKidemYili: 15,
                            izinGunSayisi: 20,
                            aciklama: '5-15 yıl arası çalışanlar',
                            aktif: true
                        },
                        {
                            id: 3,
                            minKidemYili: 15,
                            maxKidemYili: null,
                            izinGunSayisi: 26,
                            aciklama: '15 yıl üzeri çalışanlar',
                            aktif: true
                        }
                    ]
                });
            }, 500);
        });
    }
}

export default new IzinKonfigurasyonService();