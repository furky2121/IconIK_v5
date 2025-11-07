/* eslint-disable @next/next/no-img-element */

import React, { useContext, useState, useEffect } from 'react';
import AppMenuitem from './AppMenuitem';
import { LayoutContext } from './context/layoutcontext';
import { MenuProvider } from './context/menucontext';
import Link from 'next/link';
import { AppMenuItem } from '@/types';
import yetkiService from '../src/services/yetkiService';

const AppMenu = () => {
    const { layoutConfig } = useContext(LayoutContext);
    const [filteredModel, setFilteredModel] = useState<AppMenuItem[]>([]);

    const fullModel: AppMenuItem[] = [
        {
            label: 'ANA MENÜ',
            items: [
                { label: 'Dashboard', icon: 'pi pi-fw pi-home', to: '/', screenCode: 'dashboard' },
                { label: 'Organizasyon Şeması', icon: 'pi pi-fw pi-share-alt', to: '/organizasyon-semasi', screenCode: 'organizasyon-semasi' },
                { label: 'Bana Atanan Eğitimler', icon: 'pi pi-fw pi-book', to: '/bana-atanan-egitimler', screenCode: 'egitimler' },
                { label: 'Bana Atanan Anketler', icon: 'pi pi-fw pi-inbox', to: '/bana-atanan-anketler', screenCode: 'anketler' }
            ]
        },
        {
            label: 'İnsan Kaynakları',
            items: [
                {
                    label: 'Organizasyon',
                    icon: 'pi pi-fw pi-sitemap',
                    items: [
                        { label: 'Kademeler', icon: 'pi pi-fw pi-list', to: '/kademeler', screenCode: 'kademeler' },
                        { label: 'Departmanlar', icon: 'pi pi-fw pi-building', to: '/departmanlar', screenCode: 'departmanlar' },
                        { label: 'Pozisyonlar', icon: 'pi pi-fw pi-briefcase', to: '/pozisyonlar', screenCode: 'pozisyonlar' }
                    ]
                },
                {
                    label: 'İşe Alım',
                    icon: 'pi pi-fw pi-user-plus',
                    items: [
                        { label: 'İş İlanları', icon: 'pi pi-fw pi-megaphone', to: '/is-ilanlari', screenCode: 'is-ilanlari' },
                        { label: 'İlan Kategorileri', icon: 'pi pi-fw pi-tags', to: '/ilan-kategorileri', screenCode: 'ilan-kategorileri' },
                        { label: 'Özgeçmiş Havuzu', icon: 'pi pi-fw pi-database', to: '/ozgecmis-havuzu', screenCode: 'ozgecmis-havuzu' },
                        { label: 'Başvuru Yönetimi', icon: 'pi pi-fw pi-inbox', to: '/basvuru-yonetimi', screenCode: 'basvuru-yonetimi' },
                        { label: 'Mülakat Takvimi', icon: 'pi pi-fw pi-calendar-times', to: '/mulakat-takvimi', screenCode: 'mulakat-takvimi' },
                        { label: 'İşe Alım Süreçleri', icon: 'pi pi-fw pi-cog', to: '/ise-alim-surecleri', screenCode: 'ise-alim-surecleri' },
                        { label: 'Teklif Yönetimi', icon: 'pi pi-fw pi-file-edit', to: '/teklif-yonetimi', screenCode: 'teklif-yonetimi' }
                    ]
                },
                {
                    label: 'Personel Yönetimi',
                    icon: 'pi pi-fw pi-users',
                    items: [
                        { label: 'Personeller', icon: 'pi pi-fw pi-user', to: '/personeller', screenCode: 'personeller' }
                    ]
                },
                {
                    label: 'İzin İşlemleri',
                    icon: 'pi pi-fw pi-calendar',
                    items: [
                        { label: 'İzin Talepleri', icon: 'pi pi-fw pi-calendar-minus', to: '/izin-talepleri', screenCode: 'izin-talepleri' },
                        { label: 'İzin Takvimi', icon: 'pi pi-fw pi-calendar-plus', to: '/izin-takvimi', screenCode: 'izin-takvimi' },
                        { label: 'Onay Bekleyen İzin İşlemleri', icon: 'pi pi-fw pi-clock', to: '/onay-bekleyen-izin-islemleri', screenCode: 'bekleyen-izin-talepleri' }
                    ]
                },
                {
                    label: 'Eğitim Yönetimi',
                    icon: 'pi pi-fw pi-bookmark',
                    items: [
                        { label: 'Video Eğitimler', icon: 'pi pi-fw pi-play', to: '/egitimler', screenCode: 'egitimler' },
                        { label: 'Kategori Yönetimi', icon: 'pi pi-fw pi-th-large', to: '/kategori-yonetimi', screenCode: 'kategori-yonetimi' },
                        { label: 'Eğitim Katılımları', icon: 'pi pi-fw pi-users', to: '/egitim-katilimlari', screenCode: 'egitim-katilimlari' },
                        { label: 'Eğitim Raporları', icon: 'pi pi-fw pi-chart-bar', to: '/egitim-raporlari', screenCode: 'egitim-raporlari' },
                        { label: 'Sertifikalar', icon: 'pi pi-fw pi-verified', to: '/sertifikalar', screenCode: 'sertifikalar' }
                    ]
                },
                {
                    label: 'Zimmet Yönetimi',
                    icon: 'pi pi-fw pi-box',
                    items: [
                        { label: 'Zimmet Stok', icon: 'pi pi-fw pi-list', to: '/zimmet-stok', screenCode: 'zimmet-stok' },
                        { label: 'Zimmet Stok Onay Bekleyenler', icon: 'pi pi-fw pi-clock', to: '/zimmet-stok-onay', screenCode: 'zimmet-stok-onay' },
                        { label: 'Personel Zimmet İşlemleri', icon: 'pi pi-fw pi-user-plus', to: '/personel-zimmet', screenCode: 'personel-zimmet' }
                    ]
                },
                { label: 'Personel Giriş-Çıkış', icon: 'pi pi-fw pi-clock', to: '/personel-giris-cikis', screenCode: 'personel-giris-cikis' }
            ]
        },
        {
            label: 'Diğer İşlemler',
            icon: 'pi pi-fw pi-briefcase',
            items: [
                { label: 'Avans Talepleri', icon: 'pi pi-fw pi-dollar', to: '/avans-talepleri', screenCode: 'avans-talepleri' },
                { label: 'Avans Onay', icon: 'pi pi-fw pi-check-circle', to: '/avans-onay', screenCode: 'avans-onay' },
                { label: 'Masraf Talepleri', icon: 'pi pi-fw pi-credit-card', to: '/masraf-talepleri', screenCode: 'masraf-talepleri' },
                { label: 'Masraf Onay', icon: 'pi pi-fw pi-verified', to: '/masraf-onay', screenCode: 'masraf-onay' },
                { label: 'İstifa İşlemleri', icon: 'pi pi-fw pi-sign-out', to: '/istifa-islemleri', screenCode: 'istifa-islemleri' },
                { label: 'İstifa Onay', icon: 'pi pi-fw pi-times-circle', to: '/istifa-onay', screenCode: 'istifa-onay' }
            ]
        },
        {
            label: 'BORDRO İŞLEMLERİ',
            items: [
                {
                    label: 'Parametreler & Tanımlar',
                    icon: 'pi pi-fw pi-sliders-h',
                    items: [
                        { label: 'Bordro Parametreleri', icon: 'pi pi-fw pi-cog', to: '/bordro-parametreleri', screenCode: 'bordro-parametreleri' },
                        { label: 'Ödeme Tanımları', icon: 'pi pi-fw pi-plus-circle', to: '/odeme-tanimlari', screenCode: 'odeme-tanimlari' },
                        { label: 'Kesinti Tanımları', icon: 'pi pi-fw pi-minus-circle', to: '/kesinti-tanimlari', screenCode: 'kesinti-tanimlari' }
                    ]
                },
                {
                    label: 'Puantaj İşlemleri',
                    icon: 'pi pi-fw pi-calendar-times',
                    items: [
                        { label: 'Puantaj Girişi', icon: 'pi pi-fw pi-pencil', to: '/puantaj-girisi', screenCode: 'puantaj-girisi' },
                        { label: 'Puantaj Onayı', icon: 'pi pi-fw pi-check-circle', to: '/puantaj-onayi', screenCode: 'puantaj-onayi' }
                    ]
                },
                {
                    label: 'Bordro İşlemleri',
                    icon: 'pi pi-fw pi-money-bill',
                    items: [
                        { label: 'Bordro Hazırlama', icon: 'pi pi-fw pi-calculator', to: '/bordro-hazirlama', screenCode: 'bordro-hazirlama' },
                        { label: 'Bordro Onayı', icon: 'pi pi-fw pi-verified', to: '/bordro-onayi', screenCode: 'bordro-onayi' },
                        { label: 'Bordro Görüntüleme', icon: 'pi pi-fw pi-eye', to: '/bordro-goruntuleme', screenCode: 'bordro-goruntuleme' },
                        { label: 'Luca Bordro Görüntüle', icon: 'pi pi-fw pi-file-pdf', to: '/luca-bordro-goruntule', screenCode: 'luca-bordro-goruntule' },
                        { label: 'Luca Ayarları', icon: 'pi pi-fw pi-cog', to: '/luca-ayarlari', screenCode: 'luca-ayarlari' }
                    ]
                },
                { label: 'Bordro Raporları', icon: 'pi pi-fw pi-chart-line', to: '/bordro-raporlari', screenCode: 'bordro-raporlari' },
                { label: 'Bordro Dashboard', icon: 'pi pi-fw pi-chart-bar', to: '/bordro-dashboard', screenCode: 'bordro-dashboard' }
            ]
        },
        {
            label: 'Sistem',
            items: [
                { label: 'Bildirimler', icon: 'pi pi-fw pi-bell', to: '/bildirimler', screenCode: 'bildirimler' },
                {
                    label: 'Anket İşlemleri',
                    icon: 'pi pi-fw pi-chart-bar',
                    items: [
                        { label: 'Anket Yönetimi', icon: 'pi pi-fw pi-list', to: '/anketler', screenCode: 'anketler' },
                        { label: 'Anket Sonuçları', icon: 'pi pi-fw pi-chart-pie', to: '/anket-sonuclari', screenCode: 'anketler' }
                    ]
                },
                { label: 'KVKK İzin Metni', icon: 'pi pi-fw pi-shield', to: '/kvkk-izin-metni', screenCode: 'kvkk-izin-metni' },
                { label: 'E-Posta Ayarları', icon: 'pi pi-fw pi-envelope', to: '/e-posta-ayarlari', screenCode: 'e-posta-ayarlari' },
                { label: 'Firma Ayarları', icon: 'pi pi-fw pi-building', to: '/firma-ayarlari', screenCode: 'firma-ayarlari' },
                { label: 'Ayarlar', icon: 'pi pi-fw pi-cog', to: '/ayarlar', screenCode: 'ayarlar' },
                { label: 'İzin Konfigürasyonları', icon: 'pi pi-fw pi-calendar-plus', to: '/izin-konfigurasyonlari', screenCode: 'izin-konfigurasyonlari' }
            ]
        }
    ];

    useEffect(() => {
        // Load user permissions when component mounts
        yetkiService.loadUserPermissions().then(() => {
            const filtered = filterMenuByPermissions(fullModel);
            setFilteredModel(filtered);
        }).catch(() => {
            // If permission loading fails, show basic menu
            setFilteredModel(fullModel);
        });
    }, []);

    const filterMenuByPermissions = (menuItems: AppMenuItem[]): AppMenuItem[] => {
        return menuItems.map(item => {
            const filteredItem = { ...item };

            if (item.items) {
                // Filter sub-items recursively
                const filteredSubItems = filterMenuByPermissions(item.items);
                
                // Only keep parent if it has visible sub-items
                if (filteredSubItems.length > 0) {
                    filteredItem.items = filteredSubItems;
                    return filteredItem;
                } else {
                    return null;
                }
            } else if (item.screenCode) {
                // Check if user has permission for this screen
                const hasPermission = yetkiService.hasScreenPermission(item.screenCode, 'read');
                return hasPermission ? filteredItem : null;
            } else {
                // No screenCode, allow by default
                return filteredItem;
            }
        }).filter(item => item !== null) as AppMenuItem[];
    };

    return (
        <MenuProvider>
            <ul className="layout-menu">
                {filteredModel.map((item, i) => {
                    return !item?.seperator ? <AppMenuitem item={item} root={true} index={i} key={item.label} /> : <li className="menu-separator"></li>;
                })}
            </ul>
        </MenuProvider>
    );
};

export default AppMenu;
