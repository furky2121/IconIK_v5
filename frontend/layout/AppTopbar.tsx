/* eslint-disable @next/next/no-img-element */

import Link from 'next/link';
import { classNames } from 'primereact/utils';
import React, { forwardRef, useContext, useImperativeHandle, useRef, useState, useEffect } from 'react';
import { AppTopbarRef } from '@/types';
import { LayoutContext } from './context/layoutcontext';
import { Avatar } from 'primereact/avatar';
import { Menu } from 'primereact/menu';
import authService from '../src/services/authService';
import fileUploadService from '../src/services/fileUploadService';
import NotificationCenter from '../src/components/NotificationCenter';
import firmaAyarlariService from '../src/services/firmaAyarlariService';
import SifreDegistirDialog from '../src/components/SifreDegistirDialog';

const AppTopbar = forwardRef<AppTopbarRef>((props, ref) => {
    const { layoutConfig, layoutState, onMenuToggle, showProfileSidebar } = useContext(LayoutContext);
    const menubuttonRef = useRef(null);
    const topbarmenuRef = useRef(null);
    const topbarmenubuttonRef = useRef(null);
    const profileMenuRef = useRef<Menu>(null);
    const [currentUser, setCurrentUser] = useState<any>(null);
    const [isProfileMenuOpen, setIsProfileMenuOpen] = useState(false);
    const [firmaAyarlari, setFirmaAyarlari] = useState<any>(null);
    const [sifreDegistirVisible, setSifreDegistirVisible] = useState(false);

    useEffect(() => {
        // Gerçek kullanıcı bilgilerini localStorage'dan al
        const user = authService.getUser();
        if (user) {
            // Case-insensitive field access
            const personel = user.Personel || user.personel;
            
            setCurrentUser({
                ad: personel?.ad || personel?.Ad || 'Kullanıcı',
                soyad: personel?.soyad || personel?.Soyad || '',
                kullaniciAdi: user.kullaniciAdi,
                pozisyon: personel?.pozisyon?.ad || personel?.Pozisyon?.Ad || '',
                departman: personel?.pozisyon?.departman || personel?.Pozisyon?.Departman || '',
                fotografUrl: personel?.fotografUrl || personel?.FotografUrl || null,
                kademeSeviye: personel?.pozisyon?.kademe?.seviye || personel?.Pozisyon?.Kademe?.Seviye || 0
            });
        }
        
        // Avatar cache yenilendiğinde topbar'ı da yenile
        const handleAvatarRefresh = () => {
            const updatedUser = authService.getUser();
            if (updatedUser) {
                const personel = updatedUser.Personel || updatedUser.personel;
                setCurrentUser({
                    ad: personel?.ad || personel?.Ad || 'Kullanıcı',
                    soyad: personel?.soyad || personel?.Soyad || '',
                    kullaniciAdi: updatedUser.kullaniciAdi,
                    pozisyon: personel?.pozisyon?.ad || personel?.Pozisyon?.Ad || '',
                    departman: personel?.pozisyon?.departman || personel?.Pozisyon?.Departman || '',
                    fotografUrl: personel?.fotografUrl || personel?.FotografUrl || null,
                    kademeSeviye: personel?.pozisyon?.kademe?.seviye || personel?.Pozisyon?.Kademe?.Seviye || 0
                });
            }
        };

        // Avatar refresh event listener'ı ekle
        window.addEventListener('avatarRefresh', handleAvatarRefresh);

        // Firma ayarlarını yükle
        const loadFirmaAyarlari = async () => {
            try {
                const response = await firmaAyarlariService.get();
                if (response.success && response.data) {
                    setFirmaAyarlari(response.data);
                }
            } catch (error) {
                console.error('Firma ayarları yüklenemedi:', error);
            }
        };
        loadFirmaAyarlari();

        return () => {
            window.removeEventListener('avatarRefresh', handleAvatarRefresh);
        };
    }, []);

    useImperativeHandle(ref, () => ({
        menubutton: menubuttonRef.current,
        topbarmenu: topbarmenuRef.current,
        topbarmenubutton: topbarmenubuttonRef.current
    }));

    const profileMenuItems = [
        {
            label: 'Profilim',
            icon: 'pi pi-user',
            command: () => {
                window.location.href = '/profil';
            }
        },
        {
            label: 'Şifre Değiştir',
            icon: 'pi pi-lock',
            command: () => {
                setSifreDegistirVisible(true);
            }
        },
        // Sadece Genel Müdür (seviye 1) için Ayarlar menüsü
        ...(currentUser?.kademeSeviye === 1 ? [{
            separator: true
        }, {
            label: 'Sistem Ayarları',
            icon: 'pi pi-cog',
            command: () => {
                window.location.href = '/ayarlar';
            }
        }] : []),
        {
            separator: true
        },
        {
            label: 'Çıkış Yap',
            icon: 'pi pi-sign-out',
            command: () => {
                localStorage.removeItem('token');
                localStorage.removeItem('user');
                window.location.href = '/auth/login';
            }
        }
    ];

    const showProfileMenu = (event: React.MouseEvent) => {
        profileMenuRef.current?.toggle(event);
        setIsProfileMenuOpen(!isProfileMenuOpen);
    };

    return (
        <div className="layout-topbar">
            <Link href="/" className="layout-topbar-logo">
                <img src="/layout/images/icon_ik.png" width="110px" height={'82px'} alt="logo" style={{ objectFit: 'contain' }} />
                <span style={{ alignSelf: 'flex-end', marginBottom: '-1px' }}>İnsan Kaynakları</span>
            </Link>

            <button ref={menubuttonRef} type="button" className="p-link layout-menu-button layout-topbar-button" onClick={onMenuToggle}>
                <i className="pi pi-bars" />
            </button>

            {/* Firma Logosu ve Adı */}
            {firmaAyarlari && (
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginLeft: '1rem' }}>
                    {firmaAyarlari.logoUrl && (
                        <img
                            src={`${process.env.NEXT_PUBLIC_FILE_BASE_URL || 'http://localhost:5000'}${firmaAyarlari.logoUrl}`}
                            alt="Firma Logosu"
                            style={{ height: '40px', width: 'auto', objectFit: 'contain' }}
                        />
                    )}
                    <span style={{ fontSize: '0.95rem', fontWeight: '500', color: '#495057' }}>
                        Firma: {firmaAyarlari.firmaAdi}
                    </span>
                </div>
            )}

            <button ref={topbarmenubuttonRef} type="button" className="p-link layout-topbar-menu-button layout-topbar-button" onClick={showProfileSidebar}>
                <i className="pi pi-ellipsis-v" />
            </button>

            <div ref={topbarmenuRef} className={classNames('layout-topbar-menu', { 'layout-topbar-menu-mobile-active': layoutState.profileSidebarVisible })}>
                {/* Notification Center */}
                <NotificationCenter />

                {currentUser && (
                    <div className="layout-topbar-user" onClick={showProfileMenu} style={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '0.5rem', padding: '0.5rem' }}>
                        {currentUser.fotografUrl ? (
                            <Avatar 
                                image={fileUploadService.getAvatarUrl(currentUser.fotografUrl)} 
                                shape="circle" 
                                size="normal"
                                onImageError={(e: any) => {
                                    // Resim yüklenemezse initials göster - güvenli şekilde
                                    const target = e.target;
                                    const parent = target?.parentElement;
                                    if (target && parent) {
                                        target.style.display = 'none';
                                        parent.innerHTML = `<span class="p-avatar p-component p-avatar-circle" style="background-color: #2196F3; color: #ffffff; width: 2rem; height: 2rem; font-size: 1rem;"><span class="p-avatar-text">${currentUser.ad.charAt(0).toUpperCase()}${currentUser.soyad.charAt(0).toUpperCase()}</span></span>`;
                                    }
                                }}
                            />
                        ) : (
                            <Avatar 
                                label={`${currentUser.ad.charAt(0).toUpperCase()}${currentUser.soyad.charAt(0).toUpperCase()}`}
                                size="normal" 
                                shape="circle"
                                style={{ backgroundColor: '#2196F3', color: '#ffffff' }}
                            />
                        )}
                        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-start' }}>
                            <span style={{ fontSize: '0.875rem', fontWeight: '600' }}>
                                {currentUser.ad} {currentUser.soyad}
                            </span>
                            <span style={{ fontSize: '0.75rem', color: 'var(--text-color-secondary)' }}>
                                {currentUser.pozisyon}
                            </span>
                        </div>
                        <i className={`pi ${isProfileMenuOpen ? 'pi-chevron-up' : 'pi-chevron-down'}`} 
                           style={{ fontSize: '0.75rem', color: 'var(--text-color-secondary)', marginLeft: '0.25rem' }}></i>
                    </div>
                )}
                
                <Menu
                    ref={profileMenuRef}
                    model={profileMenuItems}
                    popup
                    onHide={() => setIsProfileMenuOpen(false)}
                />
            </div>

            {/* Şifre Değiştir Dialog */}
            <SifreDegistirDialog
                visible={sifreDegistirVisible}
                onHide={() => setSifreDegistirVisible(false)}
            />
        </div>
    );
});

AppTopbar.displayName = 'AppTopbar';

export default AppTopbar;
