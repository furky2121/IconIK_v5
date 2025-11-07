import React, { useState, useEffect, useRef } from 'react';
import { Card } from 'primereact/card';
import { InputText } from 'primereact/inputtext';
import { InputTextarea } from 'primereact/inputtextarea';
import { Calendar } from 'primereact/calendar';
import { Dropdown } from 'primereact/dropdown';
import { Checkbox } from 'primereact/checkbox';
import { Button } from 'primereact/button';
import { Toast } from 'primereact/toast';
import { Panel } from 'primereact/panel';
import { RadioButton } from 'primereact/radiobutton';
import anketService from '../services/anketService';
import yetkiService from '../services/yetkiService';
import { useRouter } from 'next/navigation';

const AnketOlustur = () => {
    const [anket, setAnket] = useState({
        baslik: '',
        aciklama: '',
        baslangicTarihi: null,
        bitisTarihi: null,
        anketDurumu: 'Taslak',
        anonymousMu: false,
        sorular: []
    });
    const [sorular, setSorular] = useState([]);
    const [loading, setLoading] = useState(false);
    const [permissions, setPermissions] = useState({ write: false });
    const toast = useRef(null);
    const router = useRouter();

    const durumOptions = [
        { label: 'Taslak', value: 'Taslak' },
        { label: 'Aktif', value: 'Aktif' },
        { label: 'Tamamlandı', value: 'Tamamlandı' }
    ];

    const soruTipiOptions = [
        { label: 'Tek Seçim', value: 'TekSecim' },
        { label: 'Çoklu Seçim', value: 'CokluSecim' },
        { label: 'Açık Uçlu', value: 'AcikUclu' }
    ];

    useEffect(() => {
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                write: yetkiService.hasScreenPermission('anketler', 'write')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
        }
    };

    const yeniSoruEkle = () => {
        const yeniSoru = {
            id: Date.now(),
            soruMetni: '',
            soruTipi: 'TekSecim',
            sira: sorular.length + 1,
            zorunluMu: false,
            secenekler: []
        };
        setSorular([...sorular, yeniSoru]);
    };

    const soruSil = (soruId) => {
        setSorular(sorular.filter(s => s.id !== soruId));
    };

    const soruGuncelle = (soruId, field, value) => {
        setSorular(sorular.map(s =>
            s.id === soruId ? { ...s, [field]: value } : s
        ));
    };

    const secenekEkle = (soruId) => {
        setSorular(sorular.map(s => {
            if (s.id === soruId) {
                const yeniSecenek = {
                    id: Date.now(),
                    secenekMetni: '',
                    sira: (s.secenekler?.length || 0) + 1
                };
                return {
                    ...s,
                    secenekler: [...(s.secenekler || []), yeniSecenek]
                };
            }
            return s;
        }));
    };

    const secenekSil = (soruId, secenekId) => {
        setSorular(sorular.map(s => {
            if (s.id === soruId) {
                return {
                    ...s,
                    secenekler: s.secenekler.filter(sec => sec.id !== secenekId)
                };
            }
            return s;
        }));
    };

    const secenekGuncelle = (soruId, secenekId, value) => {
        setSorular(sorular.map(s => {
            if (s.id === soruId) {
                return {
                    ...s,
                    secenekler: s.secenekler.map(sec =>
                        sec.id === secenekId ? { ...sec, secenekMetni: value } : sec
                    )
                };
            }
            return s;
        }));
    };

    const kaydet = async () => {
        // Validasyon
        const validation = anketService.validateAnket({
            ...anket,
            sorular: sorular.map(s => ({ ...s, secenekler: s.secenekler || [] }))
        });

        if (!validation.isValid) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: validation.errors.join('\n'),
                life: 5000
            });
            return;
        }

        setLoading(true);
        try {
            const userStr = localStorage.getItem('user');
            if (!userStr) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: 'Oturum bilgisi bulunamadı. Lütfen tekrar giriş yapın.',
                    life: 3000
                });
                setLoading(false);
                return;
            }

            const user = JSON.parse(userStr);
            const personelId = user?.personel?.id;

            if (!personelId) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: 'Kullanıcı bilgisi hatalı. Lütfen tekrar giriş yapın.',
                    life: 3000
                });
                setLoading(false);
                return;
            }

            const anketData = {
                ...anket,
                olusturanPersonelId: personelId,
                sorular: sorular.map(s => ({
                    soruMetni: s.soruMetni,
                    soruTipi: s.soruTipi,
                    sira: s.sira,
                    zorunluMu: s.zorunluMu,
                    secenekler: s.soruTipi !== 'AcikUclu' ? s.secenekler : []
                }))
            };

            const response = await anketService.createAnket(anketData);

            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Anket başarıyla oluşturuldu',
                    life: 3000
                });
                setTimeout(() => router.push('/anketler'), 1500);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Anket oluşturulurken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    if (!permissions.write) {
        return (
            <div className="flex align-items-center justify-content-center" style={{ minHeight: '400px' }}>
                <Card>
                    <div className="text-center">
                        <i className="pi pi-lock" style={{ fontSize: '3rem', color: 'var(--primary-color)' }}></i>
                        <h3>Yetkiniz Yok</h3>
                        <p>Anket oluşturmak için yetkiniz bulunmamaktadır.</p>
                    </div>
                </Card>
            </div>
        );
    }

    return (
        <div className="grid">
            <div className="col-12">
                <Card title="Yeni Anket Oluştur">
                    <Toast ref={toast} />

                    <div className="p-fluid formgrid grid">
                        <div className="field col-12 md:col-6">
                            <label htmlFor="baslik">Anket Başlığı *</label>
                            <InputText
                                id="baslik"
                                value={anket.baslik}
                                onChange={(e) => setAnket({ ...anket, baslik: e.target.value })}
                            />
                        </div>

                        <div className="field col-12 md:col-6">
                            <label htmlFor="durum">Durum</label>
                            <Dropdown
                                id="durum"
                                value={anket.anketDurumu}
                                options={durumOptions}
                                onChange={(e) => setAnket({ ...anket, anketDurumu: e.value })}
                            />
                        </div>

                        <div className="field col-12">
                            <label htmlFor="aciklama">Açıklama</label>
                            <InputTextarea
                                id="aciklama"
                                value={anket.aciklama}
                                onChange={(e) => setAnket({ ...anket, aciklama: e.target.value })}
                                rows={3}
                            />
                        </div>

                        <div className="field col-12 md:col-6">
                            <label htmlFor="baslangic">Başlangıç Tarihi *</label>
                            <Calendar
                                id="baslangic"
                                value={anket.baslangicTarihi}
                                onChange={(e) => setAnket({ ...anket, baslangicTarihi: e.value })}
                                showTime
                                dateFormat="dd/mm/yy"
                            />
                        </div>

                        <div className="field col-12 md:col-6">
                            <label htmlFor="bitis">Bitiş Tarihi *</label>
                            <Calendar
                                id="bitis"
                                value={anket.bitisTarihi}
                                onChange={(e) => setAnket({ ...anket, bitisTarihi: e.value })}
                                showTime
                                dateFormat="dd/mm/yy"
                            />
                        </div>

                        <div className="field col-12">
                            <div className="field-checkbox">
                                <Checkbox
                                    inputId="anonymous"
                                    checked={anket.anonymousMu}
                                    onChange={(e) => setAnket({ ...anket, anonymousMu: e.checked })}
                                />
                                <label htmlFor="anonymous">Anonim Anket</label>
                            </div>
                        </div>
                    </div>

                    <div className="mt-4">
                        <div className="flex justify-content-between align-items-center mb-3">
                            <h4>Sorular</h4>
                            <Button
                                label="Yeni Soru Ekle"
                                icon="pi pi-plus"
                                onClick={yeniSoruEkle}
                                className="p-button-sm"
                            />
                        </div>

                        {sorular.map((soru, index) => (
                            <Panel key={soru.id} header={`Soru ${index + 1}`} className="mb-3" toggleable>
                                <div className="p-fluid formgrid grid">
                                    <div className="field col-12">
                                        <label>Soru Metni *</label>
                                        <InputTextarea
                                            value={soru.soruMetni}
                                            onChange={(e) => soruGuncelle(soru.id, 'soruMetni', e.target.value)}
                                            rows={2}
                                        />
                                    </div>

                                    <div className="field col-12 md:col-6">
                                        <label>Soru Tipi</label>
                                        <Dropdown
                                            value={soru.soruTipi}
                                            options={soruTipiOptions}
                                            onChange={(e) => soruGuncelle(soru.id, 'soruTipi', e.value)}
                                        />
                                    </div>

                                    <div className="field col-12 md:col-6">
                                        <div className="field-checkbox mt-4">
                                            <Checkbox
                                                inputId={`zorunlu-${soru.id}`}
                                                checked={soru.zorunluMu}
                                                onChange={(e) => soruGuncelle(soru.id, 'zorunluMu', e.checked)}
                                            />
                                            <label htmlFor={`zorunlu-${soru.id}`}>Zorunlu Soru</label>
                                        </div>
                                    </div>

                                    {soru.soruTipi !== 'AcikUclu' && (
                                        <div className="field col-12">
                                            <div className="flex justify-content-between align-items-center mb-2">
                                                <label>Seçenekler</label>
                                                <Button
                                                    label="Seçenek Ekle"
                                                    icon="pi pi-plus"
                                                    onClick={() => secenekEkle(soru.id)}
                                                    className="p-button-sm p-button-secondary"
                                                />
                                            </div>
                                            {soru.secenekler?.map((secenek, secIndex) => (
                                                <div key={secenek.id} className="flex gap-2 mb-2">
                                                    <InputText
                                                        value={secenek.secenekMetni}
                                                        onChange={(e) => secenekGuncelle(soru.id, secenek.id, e.target.value)}
                                                        placeholder={`Seçenek ${secIndex + 1}`}
                                                    />
                                                    <Button
                                                        icon="pi pi-trash"
                                                        className="p-button-danger"
                                                        onClick={() => secenekSil(soru.id, secenek.id)}
                                                    />
                                                </div>
                                            ))}
                                        </div>
                                    )}

                                    <div className="field col-12">
                                        <Button
                                            label="Soruyu Sil"
                                            icon="pi pi-trash"
                                            className="p-button-danger p-button-sm"
                                            onClick={() => soruSil(soru.id)}
                                        />
                                    </div>
                                </div>
                            </Panel>
                        ))}
                    </div>

                    <div className="flex gap-2 mt-4">
                        <Button
                            label="Kaydet"
                            icon="pi pi-save"
                            onClick={kaydet}
                            loading={loading}
                        />
                        <Button
                            label="İptal"
                            icon="pi pi-times"
                            className="p-button-secondary"
                            onClick={() => router.push('/anketler')}
                        />
                    </div>
                </Card>
            </div>
        </div>
    );
};

export default AnketOlustur;
