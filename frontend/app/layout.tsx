'use client';
export const dynamic = 'force-dynamic';

import { LayoutProvider } from '../layout/context/layoutcontext';
import { PrimeReactProvider } from 'primereact/api';
import { ConfirmDialog } from 'primereact/confirmdialog';
import { addLocale } from 'primereact/api';
import { useEffect, Suspense } from 'react';
import { usePathname } from 'next/navigation';
import 'primereact/resources/primereact.css';
import 'primeflex/primeflex.css';
import 'primeicons/primeicons.css';
import '../styles/layout/layout.scss';
import '../styles/demo/Demos.scss';

// Türkçe locale ayarları
addLocale('tr', {
    firstDayOfWeek: 1,
    dayNames: ['Pazar', 'Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma', 'Cumartesi'],
    dayNamesShort: ['Paz', 'Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt'],
    dayNamesMin: ['Pz', 'Pt', 'Sa', 'Ça', 'Pe', 'Cu', 'Ct'],
    monthNames: ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'],
    monthNamesShort: ['Oca', 'Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem', 'Ağu', 'Eyl', 'Eki', 'Kas', 'Ara'],
    today: 'Bugün',
    clear: 'Temizle',
    dateFormat: 'dd/mm/yy',
    weekHeader: 'Hf',
    weak: 'Zayıf',
    medium: 'Orta',
    strong: 'Güçlü',
    passwordPrompt: 'Şifre giriniz',
    emptyMessage: 'Kayıt bulunamadı',
    emptyFilterMessage: 'Sonuç bulunamadı'
});

interface RootLayoutProps {
    children: React.ReactNode;
}

export default function RootLayout({ children }: RootLayoutProps) {
    return (
        <html lang="tr" suppressHydrationWarning>
            <head>
                <title>İnsan Kaynakları Yönetim Sistemi</title>
                <meta name="description" content="IconIK İK Yönetim Sistemi - Modern ve kullanıcı dostu insan kaynakları yönetimi" />
                <link id="theme-css" href={`/themes/lara-light-blue/theme.css`} rel="stylesheet"></link>
            </head>
            <body>
                <PrimeReactProvider value={{ locale: 'tr' }}>
                    <LayoutProvider>
                        <Suspense fallback={<div>Loading...</div>}>
                            <TitleManager />
                            {children}
                        </Suspense>
                    </LayoutProvider>
                    <ConfirmDialog />
                </PrimeReactProvider>
            </body>
        </html>
    );
}

// Title Manager component - pathname değişimlerini dinler
function TitleManager() {
    const pathname = usePathname();

    useEffect(() => {
        document.title = 'İnsan Kaynakları Yönetim Sistemi';
    }, [pathname]); // pathname değiştiğinde çalışır

    return null;
}
