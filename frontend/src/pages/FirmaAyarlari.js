import React, { useState, useEffect, useRef } from 'react';
import { Card } from 'primereact/card';
import { InputText } from 'primereact/inputtext';
import { Button } from 'primereact/button';
import { FileUpload } from 'primereact/fileupload';
import { Toast } from 'primereact/toast';
import { Image } from 'primereact/image';
import firmaAyarlariService from '../services/firmaAyarlariService';

const FirmaAyarlari = () => {
    const [firmaAdi, setFirmaAdi] = useState('');
    const [logoUrl, setLogoUrl] = useState(null);
    const [loading, setLoading] = useState(false);
    const [uploadedLogo, setUploadedLogo] = useState(null);
    const toast = useRef(null);
    const fileUploadRef = useRef(null);

    useEffect(() => {
        loadFirmaAyarlari();
    }, []);

    const loadFirmaAyarlari = async () => {
        try {
            const response = await firmaAyarlariService.get();
            if (response.success && response.data) {
                setFirmaAdi(response.data.firmaAdi || '');
                setLogoUrl(response.data.logoUrl);
            }
        } catch (error) {
            console.error('Firma ayarları yüklenirken hata:', error);
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Firma ayarları yüklenemedi.',
                life: 3000
            });
        }
    };

    const handleSave = async () => {
        if (!firmaAdi.trim()) {
            toast.current.show({
                severity: 'warn',
                summary: 'Uyarı',
                detail: 'Firma adı boş olamaz.',
                life: 3000
            });
            return;
        }

        setLoading(true);

        try {
            // İlk olarak logo yüklenecek mi kontrol et
            if (uploadedLogo) {
                const uploadResponse = await firmaAyarlariService.uploadLogo(uploadedLogo);
                if (uploadResponse.success) {
                    setLogoUrl(uploadResponse.data.logoUrl);
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: 'Logo yüklendi.',
                        life: 3000
                    });
                    setUploadedLogo(null);
                    if (fileUploadRef.current) {
                        fileUploadRef.current.clear();
                    }
                }
            }

            // Firma adını güncelle
            const response = await firmaAyarlariService.update({
                firmaAdi: firmaAdi
            });

            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Firma ayarları güncellendi.',
                    life: 3000
                });

                // Sayfayı yenile ki AppTopbar'da yeni bilgiler görünsün
                setTimeout(() => {
                    window.location.reload();
                }, 1500);
            }
        } catch (error) {
            console.error('Hata:', error);
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Firma ayarları güncellenemedi.',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const handleLogoDelete = async () => {
        if (!logoUrl) return;

        setLoading(true);

        try {
            const response = await firmaAyarlariService.deleteLogo();
            if (response.success) {
                setLogoUrl(null);
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Logo silindi.',
                    life: 3000
                });

                // Sayfayı yenile ki AppTopbar'da değişiklik görünsün
                setTimeout(() => {
                    window.location.reload();
                }, 1500);
            }
        } catch (error) {
            console.error('Hata:', error);
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Logo silinemedi.',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const onLogoSelect = (e) => {
        if (e.files && e.files.length > 0) {
            setUploadedLogo(e.files[0]);
            toast.current.show({
                severity: 'info',
                summary: 'Bilgi',
                detail: `Logo seçildi: ${e.files[0].name}. Kaydet butonuna basarak yükleyin.`,
                life: 3000
            });
        }
    };

    const getLogoUrl = () => {
        if (!logoUrl) return null;
        const apiBaseUrl = process.env.NEXT_PUBLIC_FILE_BASE_URL || 'http://localhost:5000';
        return `${apiBaseUrl}${logoUrl}`;
    };

    return (
        <div className="grid">
            <Toast ref={toast} />

            <div className="col-12">
                <Card title="Firma Ayarları" className="shadow-2">
                    <div className="p-fluid">
                        {/* Firma Adı */}
                        <div className="field mb-4">
                            <label htmlFor="firmaAdi" className="font-bold">
                                Firma Adı *
                            </label>
                            <InputText
                                id="firmaAdi"
                                value={firmaAdi}
                                onChange={(e) => setFirmaAdi(e.target.value)}
                                placeholder="Firma adını giriniz"
                                className="w-full"
                            />
                        </div>

                        {/* Firma Logosu */}
                        <div className="field mb-4">
                            <label className="font-bold block mb-2">Firma Logosu</label>

                            {logoUrl && (
                                <div className="mb-3">
                                    <div className="flex align-items-center gap-3">
                                        <Image
                                            src={getLogoUrl()}
                                            alt="Firma Logosu"
                                            width="120"
                                            preview
                                            className="border-round"
                                        />
                                        <Button
                                            icon="pi pi-trash"
                                            className="p-button-danger p-button-sm"
                                            label="Logoyu Sil"
                                            onClick={handleLogoDelete}
                                            disabled={loading}
                                        />
                                    </div>
                                </div>
                            )}

                            <FileUpload
                                ref={fileUploadRef}
                                mode="basic"
                                name="logo"
                                accept="image/png,image/jpeg,image/jpg,image/svg+xml"
                                maxFileSize={5000000}
                                chooseLabel={uploadedLogo ? uploadedLogo.name : "Logo Seç"}
                                onSelect={onLogoSelect}
                                auto={false}
                                className="w-full"
                            />
                            <small className="text-500">
                                PNG, JPG, JPEG veya SVG formatında, maksimum 5MB
                            </small>
                        </div>

                        {/* Kaydet Butonu */}
                        <div className="flex justify-content-end gap-2 mt-4">
                            <Button
                                label="Kaydet"
                                icon="pi pi-check"
                                className="p-button-success"
                                onClick={handleSave}
                                loading={loading}
                            />
                        </div>
                    </div>
                </Card>
            </div>
        </div>
    );
};

export default FirmaAyarlari;
