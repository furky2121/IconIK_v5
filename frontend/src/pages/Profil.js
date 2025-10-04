'use client';

import React, { useState, useEffect, useRef } from 'react';
import { Card } from 'primereact/card';
import { TabView, TabPanel } from 'primereact/tabview';
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';
import { InputTextarea } from 'primereact/inputtextarea';
import { Password } from 'primereact/password';
import { Toast } from 'primereact/toast';
import { Avatar } from 'primereact/avatar';
import { Badge } from 'primereact/badge';
import { Chip } from 'primereact/chip';
import { Panel } from 'primereact/panel';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { ProgressBar } from 'primereact/progressbar';
import { Divider } from 'primereact/divider';
import { Timeline } from 'primereact/timeline';
import { Dialog } from 'primereact/dialog';
import { FileUpload } from 'primereact/fileupload';
import { Skeleton } from 'primereact/skeleton';
import { Tag } from 'primereact/tag';
import { Knob } from 'primereact/knob';
import { ProgressSpinner } from 'primereact/progressspinner';
import { Message } from 'primereact/message';
import authService from '../services/authService';
import ApiService from '../services/api';
import fileUploadService from '../services/fileUploadService';
import './Profil.css';

const Profil = () => {
    const [kullanici, setKullanici] = useState(null);
    const [personelOzet, setPersonelOzet] = useState(null);
    const [izinDetay, setIzinDetay] = useState(null);
    const [loading, setLoading] = useState(true);
    const [activeIndex, setActiveIndex] = useState(0);
    const [sifreDegistirDialog, setSifreDegistirDialog] = useState(false);
    const [fotografDialog, setFotografDialog] = useState(false);
    const toast = useRef(null);

    // Profil güncelleme state'i
    const [profilForm, setProfilForm] = useState({
        email: '',
        telefon: '',
        adres: '',
        fotografUrl: ''
    });

    // Şifre değiştirme state'i
    const [sifreForm, setSifreForm] = useState({
        mevcutSifre: '',
        yeniSifre: '',
        yeniSifreTekrar: ''
    });

    // Authentication'dan gerçek kullanıcı ID'sini al
    const currentUser = authService.getUser();
    const kullaniciId = currentUser?.id || null;

    useEffect(() => {
        if (kullaniciId) {
            loadKullaniciProfil();
            loadPersonelOzet();
            loadIzinDetay();
        } else {
            setLoading(false);
            toast.current?.show({ 
                severity: 'error', 
                summary: 'Hata', 
                detail: 'Kullanıcı bilgileri bulunamadı. Lütfen tekrar giriş yapın.' 
            });
        }
    }, [kullaniciId]);

    const loadKullaniciProfil = async () => {
        try {
            const response = await ApiService.get(`/Profil/${kullaniciId}`);
            if (response.success) {
                setKullanici(response.data);
                setProfilForm({
                    email: response.data.personelBilgileri.email || '',
                    telefon: response.data.personelBilgileri.telefon || '',
                    adres: response.data.personelBilgileri.adres || '',
                    fotografUrl: response.data.personelBilgileri.fotografUrl || ''
                });
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Profil bilgileri yüklenemedi.' });
        }
    };

    const loadPersonelOzet = async () => {
        try {
            const response = await ApiService.get(`/Profil/PersonelOzet/${kullaniciId}`);
            if (response.success) {
                setPersonelOzet(response.data);
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Personel özeti yüklenemedi.' });
        }
    };

    const loadIzinDetay = async () => {
        try {
            const response = await ApiService.get(`/Profil/IzinDetay/${kullaniciId}`);
            if (response.success) {
                setIzinDetay(response.data);
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'İzin detayları yüklenemedi.' });
        } finally {
            setLoading(false);
        }
    };

    const updateProfil = async () => {
        try {
            const response = await ApiService.put(`/Profil/GuncelleProfil/${kullaniciId}`, profilForm);
            if (response.success) {
                toast.current?.show({ severity: 'success', summary: 'Başarılı', detail: response.message });
                loadKullaniciProfil();
            } else {
                toast.current?.show({ severity: 'error', summary: 'Hata', detail: response.message });
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Profil güncellenemedi.' });
        }
    };

    const sifreDegistir = async () => {
        if (sifreForm.yeniSifre !== sifreForm.yeniSifreTekrar) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Yeni şifre ve tekrar şifresi uyuşmuyor.' });
            return;
        }

        try {
            const response = await ApiService.put(`/Profil/SifreDegistir/${kullaniciId}`, sifreForm);
            if (response.success) {
                toast.current?.show({ severity: 'success', summary: 'Başarılı', detail: response.message });
                setSifreDegistirDialog(false);
                setSifreForm({ mevcutSifre: '', yeniSifre: '', yeniSifreTekrar: '' });
            } else {
                toast.current?.show({ severity: 'error', summary: 'Hata', detail: response.message });
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Şifre değiştirilemedi.' });
        }
    };

    const onUpload = async (event) => {
        const file = event.files[0];
        const formData = new FormData();
        formData.append('fotograf', file);

        try {
            const response = await fetch(`${ApiService.getBaseURL()}/Profil/FotografYukle/${kullaniciId}`, {
                method: 'POST',
                body: formData
            });
            
            const data = await response.json();
            
            if (data.success) {
                // localStorage'daki kullanıcı bilgisini güncelle (küçük harfle)
                const currentUser = authService.getUser();
                if (currentUser && currentUser.personel) {
                    // Backend'den gelen yeni fotoğraf URL'sini al
                    const yeniFotografUrl = data.data?.fotografUrl || data.data?.FotografUrl || data.fotografUrl;
                    currentUser.personel.fotografUrl = yeniFotografUrl;
                    authService.setUser(currentUser);
                    
                    // Avatar cache'ini temizle ve topbar'ı yenile
                    fileUploadService.refreshAvatarCache();
                }
                
                toast.current?.show({ severity: 'success', summary: 'Başarılı', detail: data.message });
                await loadKullaniciProfil();
                setFotografDialog(false);
            } else {
                toast.current?.show({ severity: 'error', summary: 'Hata', detail: data.message });
            }
        } catch (error) {
            toast.current?.show({ severity: 'error', summary: 'Hata', detail: 'Fotoğraf yüklenemedi.' });
        }
    };

    const formatCurrency = (value) => {
        return new Intl.NumberFormat('tr-TR', { 
            style: 'currency', 
            currency: 'TRY',
            minimumFractionDigits: 0 
        }).format(value || 0);
    };

    const formatDate = (dateString) => {
        return new Date(dateString).toLocaleDateString('tr-TR');
    };

    // Avatar helper - foto yoksa initials göster
    const getAvatarContent = (personelBilgileri) => {
        if (personelBilgileri?.fotografUrl) {
            const avatarUrl = fileUploadService.getAvatarUrl(personelBilgileri.fotografUrl);
            return { image: avatarUrl };
        } else {
            // Initials oluştur
            const ad = personelBilgileri?.ad || currentUser?.personel?.ad || 'U';
            const soyad = personelBilgileri?.soyad || currentUser?.personel?.soyad || 'U';
            const initials = (ad.charAt(0) + soyad.charAt(0)).toUpperCase();
            return { label: initials };
        }
    };

    const sifreDegistirDialogFooter = (
        <div className="flex justify-content-end gap-2">
            <Button 
                label="İptal" 
                icon="pi pi-times" 
                className="p-button-text" 
                onClick={() => setSifreDegistirDialog(false)} 
            />
            <Button 
                label="Değiştir" 
                icon="pi pi-check" 
                onClick={sifreDegistir} 
            />
        </div>
    );

    if (loading || !kullanici || !personelOzet || !izinDetay) {
        return (
            <div className="profil-container flex align-items-center justify-content-center" style={{ minHeight: '60vh' }}>
                <div className="text-center">
                    <ProgressSpinner style={{ width: '50px', height: '50px' }} strokeWidth="4" />
                    <p className="mt-3 text-600">Profil bilgileriniz yükleniyor...</p>
                </div>
            </div>
        );
    }

    if (!kullaniciId) {
        return (
            <div className="profil-container flex align-items-center justify-content-center" style={{ minHeight: '60vh' }}>
                <Message 
                    severity="error" 
                    text="Kullanıcı bilgileri bulunamadı. Lütfen tekrar giriş yapın." 
                    style={{ width: '100%', maxWidth: '500px' }} 
                />
            </div>
        );
    }

    return (
        <div className="profil-container">
            <Toast ref={toast} />
            
            {/* Modern Header */}
            <div className="surface-card p-4 border-round shadow-2 mb-4">
                <div className="flex align-items-center justify-content-between">
                    <div>
                        <h1 className="text-4xl font-bold text-900 m-0">Profilim</h1>
                        <p className="text-600 mt-1 mb-0">Kişisel bilgilerinizi görüntüleyin ve güncelleyin</p>
                    </div>
                    <div className="flex align-items-center gap-2">
                        <Button 
                            label="Şifre Değiştir" 
                            icon="pi pi-lock" 
                            className="p-button-outlined"
                            onClick={() => setSifreDegistirDialog(true)} 
                        />
                        <Button 
                            label="Fotoğraf Değiştir" 
                            icon="pi pi-camera" 
                            onClick={() => setFotografDialog(true)}
                        />
                    </div>
                </div>
            </div>

            <div className="grid">
                {/* Sol Sidebar - Profil Kartı */}
                <div className="col-12 lg:col-4">
                    {/* Ana Profil Kartı */}
                    <Card className="surface-card border-round shadow-2 mb-3">
                        <div className="text-center mb-4">
                            <div className="relative inline-block">
                                <Avatar 
                                    {...getAvatarContent(kullanici?.personelBilgileri)}
                                    size="xlarge" 
                                    shape="circle"
                                    className="border-3 border-white shadow-3"
                                    style={{ 
                                        width: '120px', 
                                        height: '120px',
                                        backgroundColor: !kullanici?.personelBilgileri?.fotografUrl ? '#3B82F6' : 'transparent',
                                        color: 'white',
                                        fontSize: '2.5rem',
                                        fontWeight: 'bold'
                                    }}
                                    onImageError={(e) => {
                                        console.log('Profile avatar error:', e);
                                    }}
                                />
                            </div>
                            <h2 className="text-2xl font-bold text-900 mt-3 mb-1">
                                {kullanici?.personelBilgileri?.ad || currentUser?.personel?.ad} {kullanici?.personelBilgileri?.soyad || currentUser?.personel?.soyad}
                            </h2>
                            <p className="text-600 text-lg mb-2">{kullanici?.personelBilgileri?.pozisyonAd || currentUser?.personel?.pozisyon?.ad}</p>
                            <Badge 
                                value={kullanici?.personelBilgileri?.departmanAd || currentUser?.personel?.pozisyon?.departman?.ad} 
                                size="large"
                                className="bg-primary"
                            />
                        </div>
                        
                        <Divider />
                        
                        <div className="profil-details">
                            <div className="flex align-items-center p-3 border-bottom-1 surface-border">
                                <i className="pi pi-envelope text-primary text-xl mr-5"></i>
                                <div>
                                    <small className="text-600">Email</small>
                                    <p className="m-0 font-medium">{kullanici?.personelBilgileri?.email || 'Email belirtilmemiş'}</p>
                                </div>
                            </div>
                            <div className="flex align-items-center p-3 border-bottom-1 surface-border">
                                <i className="pi pi-phone text-primary text-xl mr-5"></i>
                                <div>
                                    <small className="text-600">Telefon</small>
                                    <p className="m-0 font-medium">{kullanici?.personelBilgileri?.telefon || 'Telefon belirtilmemiş'}</p>
                                </div>
                            </div>
                            <div className="flex align-items-center p-3 border-bottom-1 surface-border">
                                <i className="pi pi-calendar text-primary text-xl mr-5"></i>
                                <div>
                                    <small className="text-600">İşe Başlama</small>
                                    <p className="m-0 font-medium">{formatDate(kullanici?.personelBilgileri?.iseBaslamaTarihi)}</p>
                                </div>
                            </div>
                            {kullanici?.personelBilgileri?.yoneticiAd && (
                                <div className="flex align-items-center p-3 border-bottom-1 surface-border">
                                    <i className="pi pi-user text-primary text-xl mr-3"></i>
                                    <div>
                                        <small className="text-600">Yönetici</small>
                                        <p className="m-0 font-medium">{kullanici.personelBilgileri.yoneticiAd}</p>
                                    </div>
                                </div>
                            )}
                            <div className="flex align-items-center p-3">
                                <i className="pi pi-shield text-primary text-xl mr-3"></i>
                                <div>
                                    <small className="text-600">Sistem Rolü</small>
                                    <p className="m-0 font-medium">{kullanici?.rol || 'Personel'}</p>
                                </div>
                            </div>
                            {izinDetay && (
                                <div className="flex align-items-center p-3">
                                    <i className="pi pi-calendar-plus text-primary text-xl mr-3"></i>
                                    <div>
                                        <small className="text-600">Çalışma Yılı</small>
                                        <p className="m-0 font-medium">{izinDetay.calismaYili + 1}. yıl</p>
                                    </div>
                                </div>
                            )}
                        </div>
                    </Card>

                </div>

                {/* Ana İçerik - Modern Tab Yapısı */}
                <div className="col-12 lg:col-8">
                    <Card className="surface-card border-round shadow-2">
                        <TabView 
                            activeIndex={activeIndex} 
                            onTabChange={(e) => setActiveIndex(e.index)}
                            className="modern-tabview"
                        >
                            <TabPanel header="Genel Bakış" leftIcon="pi pi-home">
                                {personelOzet ? (
                                    <div className="p-4">
                                        {/* Performans İstatistikleri */}
                                        <div className="mb-4">
                                            <h3 className="text-2xl font-semibold text-900 mb-3">Performans Özeti</h3>
                                            <div className="grid">
                                                <div className="col-6 md:col-3">
                                                    <div className="bg-blue-50 border-round p-4 text-center h-full">
                                                        <i className="pi pi-calendar-times text-4xl text-blue-500 mb-3"></i>
                                                        <h3 className="text-2xl font-bold text-blue-700 m-0">{personelOzet.izinOzeti.toplamIzinTalebi}</h3>
                                                        <p className="text-blue-600 font-medium mt-1 mb-0">Toplam İzin</p>
                                                    </div>
                                                </div>
                                                <div className="col-6 md:col-3">
                                                    <div className="bg-green-50 border-round p-4 text-center h-full">
                                                        <i className="pi pi-book text-4xl text-green-500 mb-3"></i>
                                                        <h3 className="text-2xl font-bold text-green-700 m-0">{personelOzet.egitimOzeti.toplamEgitim}</h3>
                                                        <p className="text-green-600 font-medium mt-1 mb-0">Atanan Toplam Eğitim</p>
                                                    </div>
                                                </div>
                                                <div className="col-6 md:col-3">
                                                    <div className="bg-yellow-50 border-round p-4 text-center h-full">
                                                        <i className="pi pi-star text-4xl text-yellow-500 mb-3"></i>
                                                        <h3 className="text-2xl font-bold text-yellow-700 m-0">{personelOzet.egitimOzeti.ortalamaPuan.toFixed(1)}</h3>
                                                        <p className="text-yellow-600 font-medium mt-1 mb-0">Ortalama Puan</p>
                                                    </div>
                                                </div>
                                                <div className="col-6 md:col-3">
                                                    <div className="bg-purple-50 border-round p-4 text-center h-full">
                                                        <i className="pi pi-users text-4xl text-purple-500 mb-3"></i>
                                                        <h3 className="text-2xl font-bold text-purple-700 m-0">{personelOzet.altCalisanSayisi}</h3>
                                                        <p className="text-purple-600 font-medium mt-1 mb-0">Alt Çalışan</p>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>

                                        {/* Son Bordro Bilgisi */}
                                        {personelOzet.sonBordro && (
                                            <div className="mb-4">
                                                <h3 className="text-2xl font-semibold text-900 mb-3">Son Bordro Bilgisi</h3>
                                                <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border-round-lg p-4">
                                                    <div className="grid align-items-center">
                                                        <div className="col-12 md:col-6 lg:col-3">
                                                            <div className="text-center p-3">
                                                                <i className="pi pi-calendar text-blue-500 text-2xl mb-2"></i>
                                                                <div className="text-600 text-sm">Dönem</div>
                                                                <div className="font-bold text-900">{personelOzet.sonBordro.donemAdi}</div>
                                                            </div>
                                                        </div>
                                                        <div className="col-12 md:col-6 lg:col-3">
                                                            <div className="text-center p-3">
                                                                <i className="pi pi-money-bill text-blue-500 text-2xl mb-2"></i>
                                                                <div className="text-600 text-sm">Brüt Maaş</div>
                                                                <div className="font-bold text-blue-600">
                                                                    {formatCurrency(personelOzet.sonBordro.brutMaas)}
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <div className="col-12 md:col-6 lg:col-3">
                                                            <div className="text-center p-3">
                                                                <i className="pi pi-wallet text-green-500 text-2xl mb-2"></i>
                                                                <div className="text-600 text-sm">Net Maaş</div>
                                                                <div className="font-bold text-green-600">
                                                                    {formatCurrency(personelOzet.sonBordro.netMaas)}
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <div className="col-12 md:col-6 lg:col-3">
                                                            <div className="text-center p-3">
                                                                <Button 
                                                                    label="Tüm Bordrolarım" 
                                                                    icon="pi pi-file-pdf" 
                                                                    className="p-button-outlined p-button-primary"
                                                                    onClick={() => window.location.href = '/bordrolar'}
                                                                />
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        )}

                                        {/* Yaklaşan Eğitimler */}
                                        {personelOzet.yaklaşanEgitimler && personelOzet.yaklaşanEgitimler.length > 0 && (
                                            <div>
                                                <h3 className="text-2xl font-semibold text-900 mb-3">Yaklaşan Eğitimler</h3>
                                                <div className="grid">
                                                    {personelOzet.yaklaşanEgitimler.map((egitim) => (
                                                        <div key={egitim.id} className="col-12 md:col-6">
                                                            <div className="bg-white border-1 surface-border border-round shadow-1 p-4 h-full">
                                                                <div className="flex align-items-start justify-content-between mb-2">
                                                                    <div className="flex-1">
                                                                        <h6 className="text-900 font-semibold m-0 mb-2">{egitim.baslik}</h6>
                                                                        <div className="flex align-items-center text-600 text-sm mb-1">
                                                                            <i className="pi pi-calendar text-primary mr-2"></i>
                                                                            <span>{formatDate(egitim.baslangicTarihi)}</span>
                                                                        </div>
                                                                        {egitim.konum && (
                                                                            <div className="flex align-items-center text-600 text-sm">
                                                                                <i className="pi pi-map-marker text-primary mr-2"></i>
                                                                                <span>{egitim.konum}</span>
                                                                            </div>
                                                                        )}
                                                                    </div>
                                                                    {egitim.sureSaat && (
                                                                        <Badge value={`${egitim.sureSaat}h`} className="bg-primary" />
                                                                    )}
                                                                </div>
                                                            </div>
                                                        </div>
                                                    ))}
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                ) : (
                                    <div className="text-center p-6">
                                        <i className="pi pi-info-circle text-4xl text-400 mb-3"></i>
                                        <p className="text-600 text-lg">Personel özet bilgileri yüklenemedi.</p>
                                    </div>
                                )}
                        </TabPanel>

                        <TabPanel header="Profil Düzenle" leftIcon="pi pi-user-edit">
                            <div className="p-4">
                                <h3 className="text-2xl font-semibold text-900 mb-4">Kişisel Bilgilerimi Güncelle</h3>
                                
                                <div className="bg-blue-50 border-round-lg p-4 mb-4">
                                    <div className="flex align-items-center">
                                        <i className="pi pi-info-circle text-blue-500 mr-2"></i>
                                        <div>
                                            <p className="text-blue-700 font-medium m-0">Bilgi</p>
                                            <p className="text-blue-600 text-sm m-0 mt-1">
                                                Ad, soyad ve pozisyon bilgileriniz sadece İK departmanı tarafından değiştirilebilir.
                                            </p>
                                        </div>
                                    </div>
                                </div>
                                
                                <div className="grid">
                                    <div className="col-12 md:col-6">
                                        <div className="field mb-4">
                                            <label htmlFor="email" className="font-semibold text-900 mb-2 block">Email Adresiniz</label>
                                            <InputText 
                                                id="email"
                                                value={profilForm.email} 
                                                onChange={(e) => setProfilForm({...profilForm, email: e.target.value})}
                                                className="w-full p-3 border-round"
                                                placeholder="ornek@bilgelojistik.com"
                                            />
                                        </div>
                                    </div>
                                    
                                    <div className="col-12 md:col-6">
                                        <div className="field mb-4">
                                            <label htmlFor="telefon" className="font-semibold text-900 mb-2 block">Telefon Numaranız</label>
                                            <InputText 
                                                id="telefon"
                                                value={profilForm.telefon} 
                                                onChange={(e) => setProfilForm({...profilForm, telefon: e.target.value})}
                                                className="w-full p-3 border-round"
                                                placeholder="0555 123 45 67"
                                            />
                                        </div>
                                    </div>
                                    
                                    <div className="col-12">
                                        <div className="field mb-4">
                                            <label htmlFor="adres" className="font-semibold text-900 mb-2 block">Adres Bilgileriniz</label>
                                            <InputTextarea 
                                                id="adres"
                                                value={profilForm.adres} 
                                                onChange={(e) => setProfilForm({...profilForm, adres: e.target.value})}
                                                rows={4}
                                                className="w-full p-3 border-round"
                                                placeholder="Ev adresinizi buraya yazabilirsiniz..."
                                            />
                                        </div>
                                    </div>
                                    
                                    <div className="col-12">
                                        <div className="flex gap-3 justify-content-end">
                                            <Button 
                                                label="Değişiklikleri Kaydet" 
                                                icon="pi pi-check" 
                                                onClick={updateProfil}
                                                className="p-button-lg"
                                            />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </TabPanel>

                        <TabPanel header="Eğitim Geçmişi" leftIcon="pi pi-book">
                            <div className="p-4">
                                {personelOzet?.egitimOzeti ? (
                                    <div>
                                        <h3 className="text-2xl font-semibold text-900 mb-4">Eğitim & Gelişim Analizi</h3>
                                        
                                        {/* Eğitim İstatistikleri */}
                                        <div className="grid mb-5">
                                            <div className="col-6 md:col-4">
                                                <div className="bg-blue-50 border-round-lg p-4 text-center h-full">
                                                    <i className="pi pi-book text-blue-500 text-3xl mb-3"></i>
                                                    <div className="text-2xl font-bold text-blue-700">
                                                        {personelOzet.egitimOzeti.toplamEgitim}
                                                    </div>
                                                    <div className="text-blue-600 font-medium">Atanan Toplam Eğitim</div>
                                                </div>
                                            </div>
                                            <div className="col-6 md:col-4">
                                                <div className="bg-green-50 border-round-lg p-4 text-center h-full">
                                                    <i className="pi pi-check-circle text-green-500 text-3xl mb-3"></i>
                                                    <div className="text-2xl font-bold text-green-700">
                                                        {personelOzet.egitimOzeti.tamamlananEgitim}
                                                    </div>
                                                    <div className="text-green-600 font-medium">Tamamlanan Eğitim</div>
                                                </div>
                                            </div>
                                            <div className="col-6 md:col-4">
                                                <div className="bg-orange-50 border-round-lg p-4 text-center h-full">
                                                    <i className="pi pi-clock text-orange-500 text-3xl mb-3"></i>
                                                    <div className="text-2xl font-bold text-orange-700">
                                                        {personelOzet.egitimOzeti.toplamEgitimSaati} Saat
                                                    </div>
                                                    <div className="text-orange-600 font-medium">Toplam Saat</div>
                                                </div>
                                            </div>
                                        </div>
                                        
                                        {/* Başarı Puanı */}
                                        <div className="bg-gradient-to-r from-indigo-50 to-purple-50 border-round-lg p-6 text-center">
                                            <h4 className="text-xl font-semibold text-900 mb-4">Genel Başarı Puanınız</h4>
                                            
                                            <div className="flex align-items-center justify-content-center gap-4 mb-4">
                                                <Knob 
                                                    value={(personelOzet.egitimOzeti.ortalamaPuan / 5) * 100} 
                                                    size={100}
                                                    strokeWidth={8}
                                                    valueColor="#8b5cf6"
                                                    rangeColor="#e5e7eb"
                                                    textColor="#374151"
                                                    valueTemplate="{value}%"
                                                    className="mr-4"
                                                />
                                                <div className="text-left">
                                                    <div className="text-3xl font-bold text-purple-700 mb-1">
                                                        {personelOzet.egitimOzeti.ortalamaPuan.toFixed(1)}
                                                        <span className="text-lg text-600 ml-1">/ 5.0</span>
                                                    </div>
                                                    <div className="flex align-items-center">
                                                        {Array.from({ length: 5 }, (_, i) => (
                                                            <i 
                                                                key={i} 
                                                                className={`pi pi-star-fill mr-1 ${
                                                                    i < Math.round(personelOzet.egitimOzeti.ortalamaPuan) 
                                                                        ? 'text-yellow-400' 
                                                                        : 'text-300'
                                                                }`}
                                                            ></i>
                                                        ))}
                                                    </div>
                                                </div>
                                            </div>
                                            
                                            <p className="text-600 text-sm m-0">
                                                Tamamladığınız eğitimlerdeki ortalama başarı puanınız
                                            </p>
                                        </div>
                                    </div>
                                ) : (
                                    <div className="text-center p-6">
                                        <i className="pi pi-book text-4xl text-400 mb-3"></i>
                                        <p className="text-600 text-lg">Eğitim geçmişi bilgileriniz yüklenemedi.</p>
                                    </div>
                                )}
                            </div>
                        </TabPanel>

                        <TabPanel header="İzin Durumum" leftIcon="pi pi-calendar-times">
                            <div className="p-4">
                                {izinDetay ? (
                                    <div>
                                        <h3 className="text-2xl font-semibold text-900 mb-4">İzin Takip & Analizi</h3>
                                        
                                        {/* İzin Kullanım Oranı - Büyük Knob */}
                                        <div className="text-center mb-6">
                                            <Knob 
                                                value={izinDetay.yillikIzinHakki > 0 ? Math.round((izinDetay.kullanilanIzin / izinDetay.yillikIzinHakki) * 100) : 0} 
                                                size={150}
                                                strokeWidth={10}
                                                valueColor="#22c55e"
                                                rangeColor="#e5e7eb"
                                                textColor="#374151"
                                                valueTemplate="{value}%"
                                            />
                                            <p className="text-600 mt-3 mb-0 text-lg">İzin Kullanım Oranı</p>
                                            <p className="text-400 text-sm mt-1">
                                                {izinDetay.kullanilanIzin} / {izinDetay.yillikIzinHakki} gün kullanıldı
                                            </p>
                                        </div>
                                        
                                        {/* İzin İstatistikleri */}
                                        <div className="grid mb-5">
                                            <div className="col-6 md:col-3">
                                                <div className="bg-blue-50 border-round-lg p-4 text-center h-full">
                                                    <i className="pi pi-calendar-plus text-blue-500 text-3xl mb-3"></i>
                                                    <div className="text-2xl font-bold text-blue-700">
                                                        {izinDetay.yillikIzinHakki}
                                                    </div>
                                                    <div className="text-blue-600 font-medium">Yıllık Hakkım</div>
                                                </div>
                                            </div>
                                            <div className="col-6 md:col-3">
                                                <div className="bg-green-50 border-round-lg p-4 text-center h-full">
                                                    <i className="pi pi-check-circle text-green-500 text-3xl mb-3"></i>
                                                    <div className="text-2xl font-bold text-green-700">
                                                        {izinDetay.kalanIzin}
                                                    </div>
                                                    <div className="text-green-600 font-medium">Kalan İzin</div>
                                                </div>
                                            </div>
                                            <div className="col-6 md:col-3">
                                                <div className="bg-orange-50 border-round-lg p-4 text-center h-full">
                                                    <i className="pi pi-calendar-minus text-orange-500 text-3xl mb-3"></i>
                                                    <div className="text-2xl font-bold text-orange-700">
                                                        {izinDetay.kullanilanIzin}
                                                    </div>
                                                    <div className="text-orange-600 font-medium">Kullanılan</div>
                                                </div>
                                            </div>
                                            <div className="col-6 md:col-3">
                                                <div className="bg-purple-50 border-round-lg p-4 text-center h-full">
                                                    <i className="pi pi-clock text-purple-500 text-3xl mb-3"></i>
                                                    <div className="text-2xl font-bold text-purple-700">
                                                        {izinDetay.bekleyenIzin}
                                                    </div>
                                                    <div className="text-purple-600 font-medium">Bekleyen Talep</div>
                                                </div>
                                            </div>
                                        </div>

                                    </div>
                                ) : (
                                    <div className="text-center p-5">
                                        <i className="pi pi-info-circle text-4xl text-400 mb-3"></i>
                                        <p className="text-600">İzin bilgileri yükleniyor...</p>
                                    </div>
                                )}
                            </div>
                        </TabPanel>
                    </TabView>
                    </Card>
                </div>
            </div>

            {/* Şifre Değiştir Dialog */}
            <Dialog 
                visible={sifreDegistirDialog} 
                style={{ width: '400px' }} 
                header="Şifre Değiştir" 
                modal 
                footer={sifreDegistirDialogFooter} 
                onHide={() => setSifreDegistirDialog(false)}
            >
                <div className="grid">
                    <div className="col-12">
                        <label htmlFor="mevcutSifre">Mevcut Şifre</label>
                        <Password 
                            id="mevcutSifre"
                            value={sifreForm.mevcutSifre} 
                            onChange={(e) => setSifreForm({...sifreForm, mevcutSifre: e.target.value})}
                            feedback={false}
                            className="w-full"
                        />
                    </div>
                    
                    <div className="col-12">
                        <label htmlFor="yeniSifre">Yeni Şifre</label>
                        <Password 
                            id="yeniSifre"
                            value={sifreForm.yeniSifre} 
                            onChange={(e) => setSifreForm({...sifreForm, yeniSifre: e.target.value})}
                            className="w-full"
                        />
                    </div>
                    
                    <div className="col-12">
                        <label htmlFor="yeniSifreTekrar">Yeni Şifre Tekrar</label>
                        <Password 
                            id="yeniSifreTekrar"
                            value={sifreForm.yeniSifreTekrar} 
                            onChange={(e) => setSifreForm({...sifreForm, yeniSifreTekrar: e.target.value})}
                            feedback={false}
                            className="w-full"
                        />
                    </div>
                </div>
            </Dialog>

            {/* Fotoğraf Yükleme Dialog */}
            <Dialog 
                visible={fotografDialog} 
                style={{ width: '400px' }} 
                header="Profil Fotoğrafı Değiştir" 
                modal 
                onHide={() => setFotografDialog(false)}
            >
                <FileUpload 
                    mode="basic" 
                    accept="image/*" 
                    maxFileSize={5000000} 
                    customUpload 
                    uploadHandler={onUpload}
                    chooseLabel="Fotoğraf Seç"
                    className="w-full"
                />
                <small className="text-600 mt-2 block">
                    Maksimum dosya boyutu: 5MB. Desteklenen formatlar: JPG, PNG, GIF
                </small>
            </Dialog>
        </div>
    );
};

export default Profil;