import { ApiService } from './api';

class AnketService extends ApiService {
    constructor() {
        super();
        this.basePath = '/anket';
    }

    // ===== ANKET CRUD =====

    async getAllAnketler() {
        return await this.get(this.basePath);
    }

    async getAktifAnketler() {
        return await this.get(`${this.basePath}/aktif`);
    }

    async getAnketById(id) {
        return await this.get(`${this.basePath}/${id}`);
    }

    async createAnket(anketData) {
        return await this.post(this.basePath, anketData);
    }

    async updateAnket(id, anketData) {
        return await this.put(`${this.basePath}/${id}`, anketData);
    }

    async deleteAnket(id) {
        return await this.delete(`${this.basePath}/${id}`);
    }

    // ===== ANKET ATAMA =====

    async getAnketAtamalari(anketId) {
        return await this.get(`${this.basePath}/${anketId}/atamalar`);
    }

    async createAtama(atamaData) {
        return await this.post(`${this.basePath}/atama`, atamaData);
    }

    async deleteAtama(atamaId) {
        return await this.delete(`${this.basePath}/atama/${atamaId}`);
    }

    async getBanaAtananAnketler(personelId) {
        return await this.get(`${this.basePath}/BanaAtananlar/${personelId}`);
    }

    // ===== ANKET CEVAPLAMA =====

    async getKatilim(anketId, personelId) {
        return await this.get(`${this.basePath}/${anketId}/katilim/${personelId}`);
    }

    async baslatAnket(anketId, personelId) {
        return await this.post(`${this.basePath}/${anketId}/baslat/${personelId}`, {});
    }

    async cevapKaydet(anketId, personelId, cevaplar) {
        return await this.post(`${this.basePath}/${anketId}/cevapla/${personelId}`, { cevaplar });
    }

    async tamamlaAnket(anketId, personelId) {
        return await this.post(`${this.basePath}/${anketId}/tamamla/${personelId}`, {});
    }

    // ===== ANKET SONUÇLARI =====

    async getAnketSonuclari(anketId) {
        return await this.get(`${this.basePath}/${anketId}/sonuclar`);
    }

    async getAnketCevaplari(anketId) {
        return await this.get(`${this.basePath}/${anketId}/cevaplar`);
    }

    async getKatilimIstatistikleri(anketId) {
        return await this.get(`${this.basePath}/${anketId}/katilimIstatistikleri`);
    }

    // ===== HELPER METHODS =====

    /**
     * Anket durumu gösterim metni
     */
    getAnketDurumuBadge(durum) {
        const durumlar = {
            'Taslak': { label: 'Taslak', severity: 'info' },
            'Aktif': { label: 'Aktif', severity: 'success' },
            'Tamamlandı': { label: 'Tamamlandı', severity: 'secondary' }
        };
        return durumlar[durum] || { label: durum, severity: 'info' };
    }

    /**
     * Soru tipi gösterim metni
     */
    getSoruTipiLabel(tip) {
        const tipler = {
            'TekSecim': 'Tek Seçim',
            'CokluSecim': 'Çoklu Seçim',
            'AcikUclu': 'Açık Uçlu'
        };
        return tipler[tip] || tip;
    }

    /**
     * Atama durumu gösterim metni
     */
    getAtamaDurumuBadge(durum) {
        const durumlar = {
            'Atandı': { label: 'Atandı', severity: 'info' },
            'Tamamlandı': { label: 'Tamamlandı', severity: 'success' },
            'SuresiGecti': { label: 'Süresi Geçti', severity: 'danger' }
        };
        return durumlar[durum] || { label: durum, severity: 'info' };
    }

    /**
     * Katılım durumu gösterim metni
     */
    getKatilimDurumuBadge(durum) {
        const durumlar = {
            'Başlamadı': { label: 'Başlamadı', severity: 'secondary' },
            'Devam Ediyor': { label: 'Devam Ediyor', severity: 'warning' },
            'Tamamlandı': { label: 'Tamamlandı', severity: 'success' }
        };
        return durumlar[durum] || { label: durum, severity: 'info' };
    }

    /**
     * Tarih kontrolü - Anketin aktif olup olmadığı
     */
    isAnketAktif(anket) {
        const now = new Date();
        const baslangic = new Date(anket.baslangicTarihi);
        const bitis = new Date(anket.bitisTarihi);

        return anket.aktif &&
               anket.anketDurumu === 'Aktif' &&
               now >= baslangic &&
               now <= bitis;
    }

    /**
     * Kalan gün hesaplama
     */
    getKalanGun(bitisTarihi) {
        const now = new Date();
        const bitis = new Date(bitisTarihi);
        const diffTime = bitis - now;
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

        return diffDays > 0 ? diffDays : 0;
    }

    /**
     * Tarih formatlama
     */
    formatDate(dateString) {
        if (!dateString) return '-';
        const date = new Date(dateString);
        return date.toLocaleDateString('tr-TR', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit'
        });
    }

    /**
     * Tarih-saat formatlama
     */
    formatDateTime(dateString) {
        if (!dateString) return '-';
        const date = new Date(dateString);
        return date.toLocaleDateString('tr-TR', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    /**
     * Anket oluşturma validasyonu
     */
    validateAnket(anket) {
        const errors = [];

        if (!anket.baslik || anket.baslik.trim() === '') {
            errors.push('Anket başlığı zorunludur');
        }

        if (!anket.baslangicTarihi) {
            errors.push('Başlangıç tarihi zorunludur');
        }

        if (!anket.bitisTarihi) {
            errors.push('Bitiş tarihi zorunludur');
        }

        if (anket.baslangicTarihi && anket.bitisTarihi) {
            const baslangic = new Date(anket.baslangicTarihi);
            const bitis = new Date(anket.bitisTarihi);

            if (bitis <= baslangic) {
                errors.push('Bitiş tarihi başlangıç tarihinden sonra olmalıdır');
            }
        }

        if (!anket.sorular || anket.sorular.length === 0) {
            errors.push('En az bir soru eklemelisiniz');
        }

        if (anket.sorular) {
            anket.sorular.forEach((soru, index) => {
                if (!soru.soruMetni || soru.soruMetni.trim() === '') {
                    errors.push(`${index + 1}. sorunun metni zorunludur`);
                }

                if (soru.soruTipi !== 'AcikUclu') {
                    if (!soru.secenekler || soru.secenekler.length < 2) {
                        errors.push(`${index + 1}. soru için en az 2 seçenek eklemelisiniz`);
                    }
                }
            });
        }

        return {
            isValid: errors.length === 0,
            errors
        };
    }

    /**
     * Cevap validasyonu
     */
    validateCevaplar(anket, cevaplar) {
        const errors = [];
        const zorunluSorular = anket.sorular.filter(s => s.zorunluMu);

        zorunluSorular.forEach(soru => {
            const cevap = cevaplar.find(c => c.soruId === soru.id);

            if (!cevap) {
                errors.push(`"${soru.soruMetni}" sorusu zorunludur`);
            } else {
                if (soru.soruTipi === 'AcikUclu') {
                    if (!cevap.acikCevap || cevap.acikCevap.trim() === '') {
                        errors.push(`"${soru.soruMetni}" sorusu zorunludur`);
                    }
                } else {
                    if (!cevap.secenekId) {
                        errors.push(`"${soru.soruMetni}" sorusu zorunludur`);
                    }
                }
            }
        });

        return {
            isValid: errors.length === 0,
            errors
        };
    }
}

export default new AnketService();
