'use client';

import { Calendar as PrimeCalendar, CalendarProps } from 'primereact/calendar';
import { useEffect, useState } from 'react';
import { addLocale } from 'primereact/api';

// Türkçe locale ayarlarını component seviyesinde tanımla
// Bu, SSR sırasında hata oluşmasını önler
if (typeof window !== 'undefined') {
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
}

/**
 * TRCalendar - Türkçe locale desteği olan Calendar wrapper component
 *
 * Bu component, SSR (Server-Side Rendering) sırasında oluşabilecek
 * locale hatalarını önlemek için oluşturulmuştur.
 *
 * Kullanım:
 * ```tsx
 * import { TRCalendar } from '@/src/components/TRCalendar';
 *
 * <TRCalendar
 *   value={date}
 *   onChange={(e) => setDate(e.value)}
 *   showIcon
 *   dateFormat="dd/mm/yy"
 * />
 * ```
 *
 * Not: locale="tr" prop'u otomatik olarak eklenir, manuel eklemeye gerek yok.
 */
export function TRCalendar(props: CalendarProps) {
    const [mounted, setMounted] = useState(false);

    useEffect(() => {
        setMounted(true);
    }, []);

    // SSR sırasında null render et, sadece client-side'da Calendar'ı göster
    if (!mounted) {
        return null;
    }

    // Türkçe locale'i otomatik olarak ekle
    return <PrimeCalendar {...props} locale="tr" />;
}

// Default export
export default TRCalendar;
