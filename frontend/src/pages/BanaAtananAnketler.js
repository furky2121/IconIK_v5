import React, { useState, useEffect, useRef } from 'react';
import { Card } from 'primereact/card';
import { Button } from 'primereact/button';
import { Toast } from 'primereact/toast';
import { Tag } from 'primereact/tag';
import { Dialog } from 'primereact/dialog';
import { RadioButton } from 'primereact/radiobutton';
import { Checkbox } from 'primereact/checkbox';
import { InputTextarea } from 'primereact/inputtextarea';
import { ProgressBar } from 'primereact/progressbar';
import anketService from '../services/anketService';

const BanaAtananAnketler = () => {
    const [anketler, setAnketler] = useState([]);
    const [loading, setLoading] = useState(false);
    const [selectedAnket, setSelectedAnket] = useState(null);
    const [cevapDialog, setCevapDialog] = useState(false);
    const [cevaplar, setCevaplar] = useState({});
    const [katilim, setKatilim] = useState(null);
    const [katilimlar, setKatilimlar] = useState({});
    const toast = useRef(null);

    useEffect(() => {
        loadBanaAtananAnketler();
    }, []);

    const loadBanaAtananAnketler = async () => {
        setLoading(true);
        try {
            const user = JSON.parse(localStorage.getItem('user') || '{}');
            const personelId = user?.personel?.id;

            if (!personelId) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: 'Kullanıcı bilgisi bulunamadı',
                    life: 3000
                });
                return;
            }

            const response = await anketService.getBanaAtananAnketler(personelId);
            if (response.success) {
                setAnketler(response.data);

                // Her anket için katılım durumunu kontrol et
                const katilimMap = {};
                for (const anket of response.data) {
                    try {
                        const katilimResp = await anketService.getKatilim(anket.id, personelId);
                        if (katilimResp.success && katilimResp.data) {
                            katilimMap[anket.id] = katilimResp.data;
                        }
                    } catch (err) {
                        // Silent fail
                    }
                }
                setKatilimlar(katilimMap);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Anketler yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const openAnketDialog = async (anket) => {
        // Tamamlanmış anketi açma
        const anketKatilim = katilimlar[anket.id];
        if (anketKatilim && anketKatilim.durum === 'Tamamlandı') {
            toast.current.show({
                severity: 'info',
                summary: 'Bilgi',
                detail: 'Bu anketi zaten tamamladınız.',
                life: 3000
            });
            return;
        }

        setSelectedAnket(anket);
        setCevaplar({});

        // Katılım durumunu kontrol et
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        const personelId = user?.personel?.id;

        try {
            const katilimResponse = await anketService.getKatilim(anket.id, personelId);
            if (katilimResponse.success && katilimResponse.data) {
                setKatilim(katilimResponse.data);
            } else {
                // Ankete başlat
                const baslatResponse = await anketService.baslatAnket(anket.id, personelId);
                if (baslatResponse.success) {
                    setKatilim(baslatResponse.data);
                }
            }
        } catch (error) {
            // Silent fail
        }

        setCevapDialog(true);
    };

    const handleCevapChange = (soruId, secenekId, acikCevap, isCokluSecim = false) => {
        if (isCokluSecim) {
            // Çoklu seçim: dizi olarak sakla
            setCevaplar(prev => {
                const existingAnswer = prev[soruId] || { soruId, secenekIdler: [] };
                const secenekIdler = existingAnswer.secenekIdler || [];

                // Seçenek zaten seçiliyse, kaldır; değilse ekle
                const isSelected = secenekIdler.includes(secenekId);
                const newSecenekIdler = isSelected
                    ? secenekIdler.filter(id => id !== secenekId)
                    : [...secenekIdler, secenekId];

                return {
                    ...prev,
                    [soruId]: { soruId, secenekIdler: newSecenekIdler }
                };
            });
        } else {
            // Tek seçim veya açık uçlu
            setCevaplar(prev => ({
                ...prev,
                [soruId]: { soruId, secenekId, acikCevap }
            }));
        }
    };

    const handleAnketiGonder = async () => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        const personelId = user?.personel?.id;

        // Cevapları backend formatına dönüştür
        const cevaplarArray = [];
        Object.values(cevaplar).forEach(cevap => {
            if (cevap.secenekIdler && cevap.secenekIdler.length > 0) {
                // Çoklu seçim: Her seçenek için ayrı bir cevap nesnesi oluştur
                cevap.secenekIdler.forEach(secenekId => {
                    cevaplarArray.push({
                        soruId: cevap.soruId,
                        secenekId: secenekId
                    });
                });
            } else {
                // Tek seçim veya açık uçlu
                cevaplarArray.push({
                    soruId: cevap.soruId,
                    secenekId: cevap.secenekId || null,
                    acikCevap: cevap.acikCevap || null
                });
            }
        });

        // Zorunlu soru kontrolü
        const zorunluSorular = selectedAnket.sorular.filter(s => s.zorunluMu);
        const eksikCevaplar = zorunluSorular.filter(
            soru => !Object.values(cevaplar).find(c => c.soruId === soru.id)
        );

        if (eksikCevaplar.length > 0) {
            toast.current.show({
                severity: 'warn',
                summary: 'Uyarı',
                detail: 'Lütfen tüm zorunlu soruları cevaplayın',
                life: 3000
            });
            return;
        }

        try {
            // Cevapları kaydet
            await anketService.cevapKaydet(selectedAnket.id, personelId, cevaplarArray);

            // Anketi tamamla
            const tamamlaResponse = await anketService.tamamlaAnket(selectedAnket.id, personelId);

            if (tamamlaResponse.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Anket cevaplarınız kaydedildi',
                    life: 3000
                });
                setCevapDialog(false);

                // Katılımları güncelle
                const katilimResp = await anketService.getKatilim(selectedAnket.id, personelId);
                if (katilimResp.success && katilimResp.data) {
                    setKatilimlar(prev => ({
                        ...prev,
                        [selectedAnket.id]: katilimResp.data
                    }));
                }
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Cevaplar kaydedilirken hata oluştu',
                life: 3000
            });
        }
    };

    const renderSoru = (soru, index) => {
        const cevap = cevaplar[soru.id] || {};

        return (
            <div key={soru.id} className="mb-4 p-3 border-1 border-200 border-round">
                <h4 className="mt-0">
                    {index + 1}. {soru.soruMetni}
                    {soru.zorunluMu && <span className="text-red-500 ml-1">*</span>}
                </h4>
                <div className="text-sm text-500 mb-2">
                    <Tag value={anketService.getSoruTipiLabel(soru.soruTipi)} severity="info" />
                </div>

                {soru.soruTipi === 'AcikUclu' ? (
                    <InputTextarea
                        value={cevap.acikCevap || ''}
                        onChange={(e) => handleCevapChange(soru.id, null, e.target.value, false)}
                        rows={4}
                        className="w-full"
                        placeholder="Cevabınızı yazın..."
                    />
                ) : (
                    <div className="flex flex-column gap-2">
                        {soru.secenekler?.map(secenek => (
                            <div key={secenek.id} className="flex align-items-center">
                                {soru.soruTipi === 'TekSecim' ? (
                                    <RadioButton
                                        inputId={`secenek-${secenek.id}`}
                                        name={`soru-${soru.id}`}
                                        value={secenek.id}
                                        onChange={() => handleCevapChange(soru.id, secenek.id, null, false)}
                                        checked={cevap.secenekId === secenek.id}
                                    />
                                ) : (
                                    <Checkbox
                                        inputId={`secenek-${secenek.id}`}
                                        value={secenek.id}
                                        onChange={() => handleCevapChange(soru.id, secenek.id, null, true)}
                                        checked={(cevap.secenekIdler || []).includes(secenek.id)}
                                    />
                                )}
                                <label htmlFor={`secenek-${secenek.id}`} className="ml-2">
                                    {secenek.secenekMetni}
                                </label>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        );
    };

    const dialogFooter = (
        <div>
            <Button label="İptal" icon="pi pi-times" text onClick={() => setCevapDialog(false)} />
            <Button label="Gönder" icon="pi pi-check" onClick={handleAnketiGonder} />
        </div>
    );

    return (
        <div className="grid">
            <div className="col-12">
                <Toast ref={toast} />
                <Card title="Bana Atanan Anketler">
                    {loading ? (
                        <div className="text-center">
                            <i className="pi pi-spin pi-spinner" style={{ fontSize: '2rem' }}></i>
                        </div>
                    ) : anketler.length === 0 ? (
                        <div className="text-center p-5">
                            <i className="pi pi-inbox" style={{ fontSize: '3rem', color: 'var(--text-color-secondary)' }}></i>
                            <h3>Atanmış Anket Yok</h3>
                            <p>Şu anda size atanmış bir anket bulunmamaktadır.</p>
                        </div>
                    ) : (
                        <div className="grid">
                            {anketler.map(anket => {
                                const kalanGun = anketService.getKalanGun(anket.bitisTarihi);
                                const isAktif = anketService.isAnketAktif(anket);
                                const anketKatilim = katilimlar[anket.id];
                                const isTamamlandi = anketKatilim && anketKatilim.durum === 'Tamamlandı';

                                return (
                                    <div key={anket.id} className="col-12 md:col-6 lg:col-4">
                                        <Card className="h-full">
                                            <div className="flex flex-column h-full">
                                                <div className="mb-2">
                                                    <Tag
                                                        value={isTamamlandi ? 'Tamamlandı' : (isAktif ? 'Aktif' : 'Süresi Dolmuş')}
                                                        severity={isTamamlandi ? 'secondary' : (isAktif ? 'success' : 'danger')}
                                                    />
                                                    {anket.anonymousMu && (
                                                        <Tag value="Anonim" severity="info" className="ml-2" />
                                                    )}
                                                </div>
                                                <h3 className="mt-0">{anket.baslik}</h3>
                                                <p className="text-600">{anket.aciklama}</p>
                                                <div className="mb-2">
                                                    <div className="text-sm">
                                                        <i className="pi pi-calendar mr-1"></i>
                                                        {anketService.formatDate(anket.baslangicTarihi)} - {anketService.formatDate(anket.bitisTarihi)}
                                                    </div>
                                                    <div className="text-sm mt-1">
                                                        <i className="pi pi-question-circle mr-1"></i>
                                                        {anket.sorular?.length || 0} Soru
                                                    </div>
                                                    {kalanGun > 0 && !isTamamlandi && (
                                                        <div className="text-sm mt-1 text-orange-600">
                                                            <i className="pi pi-clock mr-1"></i>
                                                            {kalanGun} gün kaldı
                                                        </div>
                                                    )}
                                                </div>
                                                <div className="mt-auto">
                                                    <Button
                                                        label={isTamamlandi ? "Ankete katıldığınız için teşekkürler!" : "Anketi Yanıtla"}
                                                        icon={isTamamlandi ? "pi pi-check-circle" : "pi pi-pencil"}
                                                        className="w-full"
                                                        onClick={() => openAnketDialog(anket)}
                                                        disabled={!isAktif || isTamamlandi}
                                                    />
                                                </div>
                                            </div>
                                        </Card>
                                    </div>
                                );
                            })}
                        </div>
                    )}
                </Card>

                <Dialog
                    visible={cevapDialog}
                    style={{ width: '50vw' }}
                    breakpoints={{ '960px': '75vw', '641px': '95vw' }}
                    header={selectedAnket?.baslik}
                    modal
                    className="p-fluid"
                    footer={dialogFooter}
                    onHide={() => setCevapDialog(false)}
                >
                    {selectedAnket && (
                        <div>
                            {selectedAnket.aciklama && (
                                <div className="mb-4 p-3 bg-blue-50 border-round">
                                    <p className="m-0">{selectedAnket.aciklama}</p>
                                </div>
                            )}

                            {katilim && (
                                <div className="mb-4">
                                    <div className="text-sm text-600 mb-2">İlerleme</div>
                                    <ProgressBar
                                        value={(Object.keys(cevaplar).length / selectedAnket.sorular.length) * 100}
                                        showValue={false}
                                    />
                                    <div className="text-sm text-right mt-1">
                                        {Object.keys(cevaplar).length} / {selectedAnket.sorular.length} soru
                                    </div>
                                </div>
                            )}

                            {selectedAnket.sorular?.map((soru, index) => renderSoru(soru, index))}
                        </div>
                    )}
                </Dialog>
            </div>
        </div>
    );
};

export default BanaAtananAnketler;
