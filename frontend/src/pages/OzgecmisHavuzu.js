import { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Dropdown } from 'primereact/dropdown';
import { InputTextarea } from 'primereact/inputtextarea';
import { InputNumber } from 'primereact/inputnumber';
import { Calendar } from 'primereact/calendar';
import { Tag } from 'primereact/tag';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { ConfirmDialog } from 'primereact/confirmdialog';
import { Panel } from 'primereact/panel';
import { TabView, TabPanel } from 'primereact/tabview';
import { Chip } from 'primereact/chip';
import { Rating } from 'primereact/rating';
import { FileUpload } from 'primereact/fileupload';
import { ProgressBar } from 'primereact/progressbar';
import { Divider } from 'primereact/divider';
import { Card } from 'primereact/card';
import { Timeline } from 'primereact/timeline';
import { Menu } from 'primereact/menu';
import iseAlimService from '../services/iseAlimService';
import { sehirService } from '../services/sehirService';

const OzgecmisHavuzu = () => {
    const [adaylar, setAdaylar] = useState([]);
    const [adayDialog, setAdayDialog] = useState(false);
    const [adayDetailDialog, setAdayDetailDialog] = useState(false);
    const [deleteAdayDialog, setDeleteAdayDialog] = useState(false);
    const [aday, setAday] = useState({});
    const [selectedAday, setSelectedAday] = useState(null);
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState('');
    const [loading, setLoading] = useState(false);
    const [filters, setFilters] = useState({
        ad: '',
        sehir: '',
        universite: ''
    });

    // Filtrelenmiş adaylar
    const filteredAdaylar = adaylar.filter(aday => {
        return (
            (filters.ad === '' || aday.adSoyad?.toLowerCase().includes(filters.ad.toLowerCase())) &&
            (filters.sehir === '' || aday.sehir?.toLowerCase().includes(filters.sehir.toLowerCase())) &&
            (filters.universite === '' || aday.universite?.toLowerCase().includes(filters.universite.toLowerCase()))
        );
    });
    const [cvDialog, setCvDialog] = useState(false);
    const [cvYuklemeDialog, setCvYuklemeDialog] = useState(false);
    const [cvGoruntuleDialog, setCvGoruntuleDialog] = useState(false);
    const [cvListesi, setCvListesi] = useState([]);
    const [selectedCv, setSelectedCv] = useState(null);
    const [cvOlusturuluyor, setCvOlusturuluyor] = useState(false);
    const [cvYukleniyor, setCvYukleniyor] = useState(false);
    const [cvContent, setCvContent] = useState('');
    const [validationErrors, setValidationErrors] = useState({});
    const [sehirler, setSehirler] = useState([]);
    const toast = useRef(null);
    const fileUploadRef = useRef(null);

    useEffect(() => {
        loadAdaylar();
        loadSehirler();
    }, []);

    const loadAdaylar = async () => {
        setLoading(true);
        try {
            const response = await iseAlimService.getAdaylar();
            setAdaylar(response.data);
        } catch (error) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Adaylar yüklenirken hata oluştu' });
        }
        setLoading(false);
    };

    const loadSehirler = async () => {
        try {
            const response = await sehirService.getAktif();
            setSehirler(response.data.map(sehir => ({
                label: sehir.sehirAd,
                value: sehir.sehirAd
            })));
        } catch (error) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Şehirler yüklenirken hata oluştu' });
        }
    };

    const openNew = () => {
        setAday({
            ad: '',
            soyad: '',
            tcKimlik: '',
            email: '',
            telefon: '',
            dogumTarihi: null,
            cinsiyet: null,
            medeniDurum: null,
            askerlikDurumu: null,
            adres: '',
            sehir: '',
            universite: '',
            bolum: '',
            mezuniyetYili: null,
            toplamDeneyim: 0,
            linkedinUrl: '',
            notlar: '',
            aktif: true,
            egitimler: [],
            deneyimler: [],
            yetenekler: [],
            sertifikalar: [],
            diller: [],
            referanslar: [],
            projeler: [],
            hobiler: []
        });
        setSubmitted(false);
        setAdayDialog(true);
    };

    const editAday = async (adayData) => {
        try {
            // Fetch complete aday data with related entities
            const response = await iseAlimService.getAday(adayData.id);
            if (response.success) {
                const fullAday = response.data;
            // console.log('Aday verisi geldi:', fullAday); // Debug için
                setAday({
                    ...fullAday,
                    dogumTarihi: fullAday.dogumTarihi ? new Date(fullAday.dogumTarihi) : null,
                    // Backend'den gelen field isimleriyle eşleştir (büyük harfle başlıyor)
                    egitimler: fullAday.Egitimler || fullAday.egitimler || [],
                    deneyimler: fullAday.Deneyimler || fullAday.deneyimler || [],
                    yetenekler: fullAday.Yetenekler || fullAday.yetenekler || [],
                    sertifikalar: fullAday.Sertifikalar || fullAday.sertifikalar || [],
                    diller: fullAday.Diller || fullAday.diller || [],
                    referanslar: fullAday.Referanslar || fullAday.referanslar || []
                });
                // console.log('State aday:', {
                //     egitimler: fullAday.Egitimler || fullAday.egitimler,
                //     deneyimler: fullAday.Deneyimler || fullAday.deneyimler,
                //     yetenekler: fullAday.Yetenekler || fullAday.yetenekler
                // }); // Debug için
                setAdayDialog(true);
            }
        } catch (error) {
            // console.error('Aday yükleme hatası:', error);
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Aday bilgileri yüklenirken hata oluştu' });
        }
    };

    const confirmDeleteAday = (aday) => {
        setAday(aday);
        setDeleteAdayDialog(true);
    };

    const viewAdayDetail = async (adayData) => {
        try {
            const response = await iseAlimService.getAday(adayData.id);
            setSelectedAday(response.data);
            setAdayDetailDialog(true);
        } catch (error) {
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Aday detayı yüklenirken hata oluştu' });
        }
    };

    const hideDialog = () => {
        setSubmitted(false);
        setValidationErrors({});
        setAdayDialog(false);
    };

    const hideDeleteAdayDialog = () => {
        setDeleteAdayDialog(false);
    };

    const hideDetailDialog = () => {
        setAdayDetailDialog(false);
        setSelectedAday(null);
    };


    const saveAday = async () => {
        setSubmitted(true);

        // Form validasyonu
        const errors = validateForm();
        setValidationErrors(errors);

        // Eğer validasyon hatası varsa formu gönderme
        if (Object.keys(errors).length > 0) {
            toast.current.show({
                severity: 'warn',
                summary: 'Uyarı',
                detail: 'Lütfen form hatalarını düzeltin.',
                life: 3000
            });
            return;
        }

        try {
            let _adaylar = [...adaylar];
            let _aday = { ...aday };

            if (_aday.id) {
                const response = await iseAlimService.updateAday(_aday.id, _aday);
                if (response.success) {
                    const index = _adaylar.findIndex(a => a.id === _aday.id);
                    _adaylar[index] = _aday;
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: 'Aday başarıyla güncellendi.',
                        life: 3000
                    });
                }
            } else {
                const response = await iseAlimService.createAday(_aday);
                if (response.success) {
                    _aday.id = response.data.id;
                    _adaylar.push(_aday);
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: 'Aday başarıyla eklendi.',
                        life: 3000
                    });
                }
            }

            setAdaylar(_adaylar);
            setAdayDialog(false);
            setAday({});
            setValidationErrors({});
            setSubmitted(false);
            loadAdaylar(); // Refresh data
        } catch (error) {
            const errorMessage = error.response?.data?.message || 'İşlem sırasında hata oluştu';
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: errorMessage,
                life: 5000
            });

            // Backend'den gelen validasyon hatalarını işle
            if (error.response?.status === 400 && error.response?.data?.errors) {
                setValidationErrors(error.response.data.errors);
            }
        }
    };

    const deleteAday = async () => {
        try {
            const response = await iseAlimService.deleteAday(aday.id);
            if (response.success) {
                let _adaylar = adaylar.filter(a => a.id !== aday.id);
                setAdaylar(_adaylar);
                setDeleteAdayDialog(false);
                setAday({});
                toast.current.show({ severity: 'success', summary: 'Başarılı', detail: 'Aday silindi' });
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.response?.data?.message || 'Silme işlemi sırasında hata oluştu'
            });
        }
    };

    const karaListeyeEkle = async (adayData) => {
        try {
            const response = await iseAlimService.karaListeyeEkle(adayData.id);
            if (response.success) {
                loadAdaylar();
                toast.current.show({ severity: 'success', summary: 'Başarılı', detail: 'Aday kara listeye eklendi' });
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.response?.data?.message || 'İşlem sırasında hata oluştu'
            });
        }
    };

    const karaListedenCikar = async (adayData) => {
        try {
            const response = await iseAlimService.karaListedenCikar(adayData.id);
            if (response.success) {
                loadAdaylar();
                toast.current.show({ severity: 'success', summary: 'Başarılı', detail: 'Aday kara listeden çıkarıldı' });
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.response?.data?.message || 'İşlem sırasında hata oluştu'
            });
        }
    };

    // Fotoğraf yükleme fonksiyonları
    const onFileSelect = (e) => {
        const file = e.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (event) => {
                setAday(prev => ({
                    ...prev,
                    selectedFile: file,
                    fotografPreview: event.target.result
                }));
            };
            reader.readAsDataURL(file);
        }
    };

    const customUploader = async (e) => {
        if (!aday.selectedFile) return;

        try {
            // Önce aday kaydedilmeli (eğer yeni aday ise)
            if (!aday.id) {
                toast.current.show({
                    severity: 'warn',
                    summary: 'Uyarı',
                    detail: 'Önce aday bilgilerini kaydetmelisiniz'
                });
                return;
            }

            const response = await iseAlimService.fotografYukle(aday.id, aday.selectedFile);
            if (response.success) {
                setAday(prev => ({
                    ...prev,
                    fotografYolu: response.data.fotografYolu,
                    selectedFile: null,
                    fotografPreview: null
                }));

                // Aday listesini güncelle
                setAdaylar(prevAdaylar =>
                    prevAdaylar.map(a =>
                        a.id === aday.id
                            ? { ...a, fotografYolu: response.data.fotografYolu }
                            : a
                    )
                );

                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Fotoğraf başarıyla yüklendi'
                });
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.response?.data?.message || 'Fotoğraf yüklenirken hata oluştu'
            });
        }
    };

    const removeFotograf = () => {
        setAday(prev => ({
            ...prev,
            fotografYolu: '',
            selectedFile: null,
            fotografPreview: null
        }));
    };

    // PDF ve yazdırma fonksiyonları
    const handlePrintCV = () => {
        // Yazdırma için özel CSS ile stil uygula
        const printWindow = window.open('', '_blank');
        const printContent = `
            <!DOCTYPE html>
            <html>
            <head>
                <title>${selectedAday?.ad} ${selectedAday?.soyad} - CV</title>
                <style>
                    @media print {
                        body { margin: 0; }
                        .cv-container { box-shadow: none; }
                        @page { margin: 1cm; }
                    }
                    ${getCVPrintStyles()}
                </style>
            </head>
            <body>
                ${cvContent}
            </body>
            </html>
        `;
        printWindow.document.write(printContent);
        printWindow.document.close();
        printWindow.focus();
        printWindow.print();
        printWindow.close();
    };

    const handleDownloadPDF = async () => {
        if (!cvContent) return;

        try {
            // Dinamik import ile html2pdf'i client-side'da yükle
            const html2pdf = (await import('html2pdf.js')).default;

            const element = document.createElement('div');
            element.innerHTML = cvContent;
            element.style.width = '210mm';
            element.style.minHeight = '297mm';

            const opt = {
                margin: [10, 10, 10, 10],
                filename: `${selectedAday?.ad}_${selectedAday?.soyad}_CV.pdf`,
                image: { type: 'jpeg', quality: 0.98 },
                html2canvas: {
                    scale: 2,
                    useCORS: true,
                    letterRendering: true,
                    allowTaint: false
                },
                jsPDF: {
                    unit: 'mm',
                    format: 'a4',
                    orientation: 'portrait'
                }
            };

            html2pdf().set(opt).from(element).save();
        } catch (error) {
            // console.error('PDF indirme hatası:', error);
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'PDF indirilemedi'
            });
        }
    };

    const getCVPrintStyles = () => {
        return `
            * { margin: 0; padding: 0; box-sizing: border-box; }
            body { font-family: 'Arial', sans-serif; font-size: 11px; line-height: 1.3; color: #2C3E50; }
            .cv-container { display: flex; width: 100%; background: white; min-height: auto; }
            .left-column { width: 30%; background: linear-gradient(135deg, #2C3E50 0%, #34495E 100%); color: white; padding: 15px; }
            .right-column { width: 70%; padding: 15px 20px; background: white; }
            .left-column h3, .right-column h2 { font-size: 13px; margin-bottom: 8px; border-bottom: 1px solid #3498DB; padding-bottom: 3px; }
            .left-column p { margin-bottom: 6px; font-size: 10px; }
            .profile-photo { width: 80px; height: 80px; border-radius: 50%; object-fit: cover; margin: 0 auto 10px; display: block; }
            .profile-section h1 { font-size: 16px; margin-bottom: 8px; text-align: center; }
            .experience-item, .education-item { margin-bottom: 10px; padding-bottom: 8px; border-bottom: 1px solid #ECF0F1; }
            .experience-item h3, .education-item h3 { font-size: 13px; margin-bottom: 3px; }
            .experience-item h4, .education-item h4 { font-size: 11px; margin-bottom: 2px; color: #7F8C8D; }
        `;
    };

    const filtersTemizle = () => {
        setFilters({
            ad: '',
            sehir: '',
            universite: ''
        });
    };

    // CV İşlemleri
    const cvYonetimi = async (adayData) => {
        try {
            setSelectedAday(adayData);
            const response = await iseAlimService.getCVListesi(adayData.id);
            setCvListesi(response.data);
            setCvDialog(true);
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'CV listesi yüklenirken hata oluştu'
            });
        }
    };

    const otomatikCVOlustur = async () => {
        if (!selectedAday) return;

        setCvOlusturuluyor(true);
        try {
            const response = await iseAlimService.otomatikCVOlustur(selectedAday.id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Otomatik CV başarıyla oluşturuldu'
                });
                // CV listesini yenile
                const cvResponse = await iseAlimService.getCVListesi(selectedAday.id);
                setCvListesi(cvResponse.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.response?.data?.message || 'CV oluşturulurken hata oluştu'
            });
        }
        setCvOlusturuluyor(false);
    };

    const cvYukle = async (event) => {
        const file = event.files[0];
        if (!file || !selectedAday) return;

        setCvYukleniyor(true);
        try {
            const response = await iseAlimService.cvYukle(selectedAday.id, file);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'CV başarıyla yüklendi'
                });
                // CV listesini yenile
                const cvResponse = await iseAlimService.getCVListesi(selectedAday.id);
                setCvListesi(cvResponse.data);
                setCvYuklemeDialog(false);
                if (fileUploadRef.current) {
                    fileUploadRef.current.clear();
                }
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.response?.data?.message || 'CV yüklenirken hata oluştu'
            });
        }
        setCvYukleniyor(false);
    };

    const cvGoruntule = async (cv) => {
        try {
            const response = await iseAlimService.getCVGoruntule(selectedAday.id, cv.cvTipi);
            if (response.success) {
                setCvContent(response.data);
                setSelectedCv(cv);
                setCvGoruntuleDialog(true);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'CV görüntülenirken hata oluştu'
            });
        }
    };

    const cvSil = async (cv) => {
        try {
            const response = await iseAlimService.cvSil(cv.id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'CV başarıyla silindi'
                });
                // CV listesini yenile
                const cvResponse = await iseAlimService.getCVListesi(selectedAday.id);
                setCvListesi(cvResponse.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'CV silinirken hata oluştu'
            });
        }
    };

    const hideCvDialog = () => {
        setCvDialog(false);
        setSelectedAday(null);
        setCvListesi([]);
    };

    const hideCvYuklemeDialog = () => {
        setCvYuklemeDialog(false);
        if (fileUploadRef.current) {
            fileUploadRef.current.clear();
        }
    };

    const hideCvGoruntuleDialog = () => {
        setCvGoruntuleDialog(false);
        setSelectedCv(null);
        setCvContent('');
    };

    const onInputChange = (e, name) => {
        const val = (e.target && e.target.value) || '';
        let _aday = { ...aday };
        _aday[`${name}`] = val;
        setAday(_aday);
    };

    const onInputNumberChange = (e, name) => {
        const val = e.value || null;
        let _aday = { ...aday };
        _aday[`${name}`] = val;
        setAday(_aday);
    };

    const onDropdownChange = (e, name) => {
        const val = e.value;
        let _aday = { ...aday };
        _aday[`${name}`] = val;
        setAday(_aday);
    };

    const onDateChange = (e, name) => {
        const val = e.value;
        let _aday = { ...aday };
        _aday[`${name}`] = val;
        setAday(_aday);
    };

    const onFilterChange = (e, name) => {
        const val = (e.target && e.target.value) || '';
        let _filters = { ...filters };
        _filters[`${name}`] = val;
        setFilters(_filters);
    };


    // Dynamic array management functions
    const addDeneyim = () => {
        const newDeneyim = {
            sirketAd: '',
            pozisyon: '',
            baslangicTarihi: null,
            bitisTarihi: null,
            halenCalisiyor: false,
            aciklama: ''
        };
        setAday(prev => ({
            ...prev,
            deneyimler: [...(prev.deneyimler || []), newDeneyim]
        }));
    };

    const removeDeneyim = (index) => {
        setAday(prev => ({
            ...prev,
            deneyimler: prev.deneyimler.filter((_, i) => i !== index)
        }));
    };

    const updateDeneyim = (index, field, value) => {
        setAday(prev => ({
            ...prev,
            deneyimler: prev.deneyimler.map((item, i) =>
                i === index ? { ...item, [field]: value } : item
            )
        }));
    };

    const addEgitim = () => {
        const newEgitim = {
            okulAd: '',
            bolum: '',
            derece: '',
            baslangicYili: new Date().getFullYear(),
            mezuniyetYili: null,
            devamEdiyor: false,
            notOrtalamasi: null,
            aciklama: ''
        };
        setAday(prev => ({
            ...prev,
            egitimler: [...(prev.egitimler || []), newEgitim]
        }));
    };

    const removeEgitim = (index) => {
        setAday(prev => ({
            ...prev,
            egitimler: prev.egitimler.filter((_, i) => i !== index)
        }));
    };

    const updateEgitim = (index, field, value) => {
        setAday(prev => ({
            ...prev,
            egitimler: prev.egitimler.map((item, i) =>
                i === index ? { ...item, [field]: value } : item
            )
        }));
    };

    const addYetenek = () => {
        const newYetenek = {
            yetenek: '',
            seviye: 1
        };
        setAday(prev => ({
            ...prev,
            yetenekler: [...(prev.yetenekler || []), newYetenek]
        }));
    };

    const removeYetenek = (index) => {
        setAday(prev => ({
            ...prev,
            yetenekler: prev.yetenekler.filter((_, i) => i !== index)
        }));
    };

    const updateYetenek = (index, field, value) => {
        setAday(prev => ({
            ...prev,
            yetenekler: prev.yetenekler.map((item, i) =>
                i === index ? { ...item, [field]: value } : item
            )
        }));
    };

    const addSertifika = () => {
        const newSertifika = {
            sertifikaAd: '',
            verenKurum: '',
            tarih: null,
            gecerlilikTarihi: null,
            sertifikaNo: '',
            aciklama: ''
        };
        setAday(prev => ({
            ...prev,
            sertifikalar: [...(prev.sertifikalar || []), newSertifika]
        }));
    };

    const removeSertifika = (index) => {
        setAday(prev => ({
            ...prev,
            sertifikalar: prev.sertifikalar.filter((_, i) => i !== index)
        }));
    };

    const updateSertifika = (index, field, value) => {
        setAday(prev => ({
            ...prev,
            sertifikalar: prev.sertifikalar.map((item, i) =>
                i === index ? { ...item, [field]: value } : item
            )
        }));
    };

    const addDil = () => {
        const newDil = {
            dil: '',
            okumaSeviyesi: 1,
            yazmaSeviyesi: 1,
            konusmaSeviyesi: 1,
            sertifika: '',
            sertifikaPuani: ''
        };
        setAday(prev => ({
            ...prev,
            diller: [...(prev.diller || []), newDil]
        }));
    };

    const removeDil = (index) => {
        setAday(prev => ({
            ...prev,
            diller: prev.diller.filter((_, i) => i !== index)
        }));
    };

    const updateDil = (index, field, value) => {
        setAday(prev => ({
            ...prev,
            diller: prev.diller.map((item, i) =>
                i === index ? { ...item, [field]: value } : item
            )
        }));
    };

    const addReferans = () => {
        const newReferans = {
            adSoyad: '',
            sirket: '',
            pozisyon: '',
            telefon: '',
            email: '',
            iliskiTuru: '',
            aciklama: ''
        };
        setAday(prev => ({
            ...prev,
            referanslar: [...(prev.referanslar || []), newReferans]
        }));
    };

    const removeReferans = (index) => {
        setAday(prev => ({
            ...prev,
            referanslar: prev.referanslar.filter((_, i) => i !== index)
        }));
    };

    const updateReferans = (index, field, value) => {
        setAday(prev => ({
            ...prev,
            referanslar: prev.referanslar.map((item, i) =>
                i === index ? { ...item, [field]: value } : item
            )
        }));
    };

    const addProje = () => {
        const newProje = {
            projeAd: '',
            rol: '',
            baslangicTarihi: null,
            bitisTarihi: null,
            devamEdiyor: false,
            teknolojiler: '',
            aciklama: '',
            projeUrl: ''
        };
        setAday(prev => ({
            ...prev,
            projeler: [...(prev.projeler || []), newProje]
        }));
    };

    const removeProje = (index) => {
        setAday(prev => ({
            ...prev,
            projeler: prev.projeler.filter((_, i) => i !== index)
        }));
    };

    const updateProje = (index, field, value) => {
        setAday(prev => ({
            ...prev,
            projeler: prev.projeler.map((item, i) =>
                i === index ? { ...item, [field]: value } : item
            )
        }));
    };

    const addHobi = () => {
        const newHobi = {
            hobi: '',
            kategori: '',
            seviye: '',
            aciklama: ''
        };
        setAday(prev => ({
            ...prev,
            hobiler: [...(prev.hobiler || []), newHobi]
        }));
    };

    const removeHobi = (index) => {
        setAday(prev => ({
            ...prev,
            hobiler: prev.hobiler.filter((_, i) => i !== index)
        }));
    };

    const updateHobi = (index, field, value) => {
        setAday(prev => ({
            ...prev,
            hobiler: prev.hobiler.map((item, i) =>
                i === index ? { ...item, [field]: value } : item
            )
        }));
    };

    // Validasyon fonksiyonları
    const validateEmail = (email) => {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    };

    const validateTCKimlik = (tcKimlik) => {
        // TC Kimlik numarası 11 haneli olmalı ve sadece rakam içermeli
        const tcRegex = /^\d{11}$/;
        return tcRegex.test(tcKimlik);
    };

    const validatePhone = (phone) => {
        if (!phone) return true; // Opsiyonel alan
        // Türk telefon numarası formatı
        const phoneRegex = /^(\+90|0)?[5][0-9]{9}$/;
        return phoneRegex.test(phone.replace(/\s/g, ''));
    };

    const validateLinkedInUrl = (url) => {
        if (!url) return true; // Opsiyonel alan
        const linkedinRegex = /^https?:\/\/(www\.)?linkedin\.com\/in\/[a-zA-Z0-9-]+\/?$/;
        return linkedinRegex.test(url);
    };

    // Helper function to get highest degree university
    const getHighestDegreeUniversity = (aday) => {
        if (!aday.egitimler || aday.egitimler.length === 0) {
            return aday.universite || '';
        }

        const degreeOrder = { 'Doktora': 5, 'Yüksek Lisans': 4, 'Lisans': 3, 'Ön Lisans': 2, 'Lise': 1 };

        let highestDegree = '';
        let highestUniversity = '';
        let highestOrder = 0;

        aday.egitimler.forEach(egitim => {
            const order = degreeOrder[egitim.derece] || 0;
            if (order > highestOrder) {
                highestOrder = order;
                highestDegree = egitim.derece;
                highestUniversity = egitim.okulAd;
            }
        });

        return highestUniversity || aday.universite || '';
    };


    const validateForm = () => {
        const errors = {};

        // Zorunlu alanlar
        if (!aday.ad?.trim()) {
            errors.ad = 'Ad gereklidir.';
        }

        if (!aday.soyad?.trim()) {
            errors.soyad = 'Soyad gereklidir.';
        }

        if (!aday.tcKimlik?.trim()) {
            errors.tcKimlik = 'TC Kimlik numarası gereklidir.';
        } else if (!validateTCKimlik(aday.tcKimlik)) {
            errors.tcKimlik = 'Geçerli bir TC Kimlik numarası giriniz (11 haneli).';
        }

        if (!aday.email?.trim()) {
            errors.email = 'Email gereklidir.';
        } else if (!validateEmail(aday.email)) {
            errors.email = 'Geçerli bir email adresi giriniz.';
        }

        // Opsiyonel alan validasyonları
        if (aday.telefon && !validatePhone(aday.telefon)) {
            errors.telefon = 'Geçerli bir telefon numarası giriniz (05xxxxxxxxx).';
        }

        if (aday.linkedinUrl && !validateLinkedInUrl(aday.linkedinUrl)) {
            errors.linkedinUrl = 'Geçerli bir LinkedIn URL\'si giriniz.';
        }


        return errors;
    };

    const leftToolbarTemplate = () => {
        return (
            <div className="flex flex-wrap gap-2">
                <Button label="Yeni Aday" icon="pi pi-plus" className="p-button-success" onClick={openNew} />
                <Button label="Tümünü Göster" icon="pi pi-refresh" className="p-button-help" onClick={loadAdaylar} />
            </div>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <div className="flex align-items-center gap-2">
                <span className="p-input-icon-left">
                    <i className="pi pi-search" />
                    <InputText
                        type="search"
                        onInput={(e) => setGlobalFilter(e.target.value)}
                        placeholder="Ara..."
                    />
                </span>
            </div>
        );
    };

    const durumBodyTemplate = (rowData) => {
        if (rowData.karaListe) {
            return <Tag value="Kara Liste" severity="danger" />;
        }
        return <Tag value={rowData.aktif ? "Aktif" : "Pasif"} severity={rowData.aktif ? "success" : "warning"} />;
    };

    const adayDurumBodyTemplate = (rowData) => {
        const durumlar = iseAlimService.getAdayDurumlari();
        const durum = durumlar.find(d => d.value === rowData.durum);
        const durumText = durum ? durum.label : 'Bilinmeyen';
        const severity = iseAlimService.getAdayDurumSeverity(rowData.durum);

        return <Tag value={durumText} severity={severity} />;
    };

    const deneyimBodyTemplate = (rowData) => {
        // Backend now provides pre-formatted experience text in deneyimYil field
        return rowData.deneyimYil || 'Yeni mezun';
    };

    const universitePBodyTemplate = (rowData) => {
        return getHighestDegreeUniversity(rowData);
    };

    const tarihBodyTemplate = (rowData) => {
        return iseAlimService.formatDateShort(rowData.createdAt);
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-1">
                <Button
                    icon="pi pi-eye"
                    className="p-button-rounded p-button-info p-button-sm"
                    onClick={() => viewAdayDetail(rowData)}
                    tooltip="Detay Görüntüle"
                />
                <Button
                    icon="pi pi-file"
                    className="p-button-rounded p-button-secondary p-button-sm"
                    onClick={() => cvYonetimi(rowData)}
                    tooltip="CV Yönetimi"
                />
                <Button
                    icon="pi pi-pencil"
                    className="p-button-rounded p-button-success p-button-sm"
                    onClick={() => editAday(rowData)}
                    tooltip="Düzenle"
                />
                {!rowData.karaListe ? (
                    <Button
                        icon="pi pi-ban"
                        className="p-button-rounded p-button-warning p-button-sm"
                        onClick={() => karaListeyeEkle(rowData)}
                        tooltip="Kara Listeye Ekle"
                    />
                ) : (
                    <Button
                        icon="pi pi-check"
                        className="p-button-rounded p-button-help p-button-sm"
                        onClick={() => karaListedenCikar(rowData)}
                        tooltip="Kara Listeden Çıkar"
                    />
                )}
                <Button
                    icon="pi pi-trash"
                    className="p-button-rounded p-button-danger p-button-sm"
                    onClick={() => confirmDeleteAday(rowData)}
                    tooltip="Sil"
                />
            </div>
        );
    };

    const adayDialogFooter = (
        <div>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label="Kaydet" icon="pi pi-check" className="p-button-text" onClick={saveAday} />
        </div>
    );

    const deleteAdayDialogFooter = (
        <div>
            <Button label="Hayır" icon="pi pi-times" className="p-button-text" onClick={hideDeleteAdayDialog} />
            <Button label="Evet" icon="pi pi-check" className="p-button-text" onClick={deleteAday} />
        </div>
    );


    return (
        <div className="card">
            <Toast ref={toast} />

            <div className="flex justify-content-between align-items-center mb-4">
                <h2>Özgeçmiş Havuzu</h2>
                <div className="flex gap-2">
                    <Tag value={`${filteredAdaylar.length} Aday`} />
                    <Tag value={`${filteredAdaylar.filter(a => a.karaListe).length} Kara Liste`} severity="danger" />
                </div>
            </div>

            <Toolbar className="mb-4" left={leftToolbarTemplate} right={rightToolbarTemplate} />

            {/* Filtreler */}
            <div className="card mb-3">
                <h5>Filtreler</h5>
                <div className="formgrid grid">
                    <div className="field col-12 md:col-4">
                        <label htmlFor="filterAd">Ad Soyad</label>
                        <InputText
                            id="filterAd"
                            value={filters.ad}
                            onChange={(e) => onFilterChange(e, 'ad')}
                            placeholder="Ad veya soyad giriniz"
                        />
                    </div>
                    <div className="field col-12 md:col-4">
                        <label htmlFor="filterSehir">Şehir</label>
                        <Dropdown
                            id="filterSehir"
                            value={filters.sehir}
                            options={[{ label: 'Tüm Şehirler', value: '' }, ...sehirler]}
                            onChange={(e) => onFilterChange(e, 'sehir')}
                            placeholder="Şehir seçiniz"
                            showClear
                        />
                    </div>
                    <div className="field col-12 md:col-4">
                        <label htmlFor="filterUniversite">Üniversite</label>
                        <InputText
                            id="filterUniversite"
                            value={filters.universite}
                            onChange={(e) => onFilterChange(e, 'universite')}
                            placeholder="Üniversite adı giriniz"
                        />
                    </div>
                </div>
                <div className="flex gap-2">
                    <Button
                        label="Filtreleri Temizle"
                        icon="pi pi-times"
                        className="p-button-outlined"
                        onClick={filtersTemizle}
                    />
                </div>
            </div>

            <DataTable
                value={filteredAdaylar}
                paginator
                rows={10}
                rowsPerPageOptions={[5, 10, 25]}
                dataKey="id"
                loading={loading}
                globalFilter={globalFilter}
                emptyMessage="Aday bulunamadı."
                className="p-datatable-gridlines"
                showGridlines
            >
                <Column field="adSoyad" header="Ad Soyad" sortable style={{ minWidth: '12rem' }} />
                <Column field="email" header="Email" sortable />
                <Column field="telefon" header="Telefon" />
                <Column field="sehir" header="Şehir" sortable />
                <Column field="universite" header="Üniversite" body={universitePBodyTemplate} sortable />
                <Column field="toplamDeneyim" header="Deneyim" body={deneyimBodyTemplate} sortable />
                <Column field="basvuruSayisi" header="Başvuru" sortable />
                <Column field="durum" header="Aday Durumu" body={adayDurumBodyTemplate} sortable />
                <Column body={durumBodyTemplate} header="Aktif Durum" />
                <Column field="createdAt" header="Kayıt Tarihi" body={tarihBodyTemplate} sortable />
                <Column body={actionBodyTemplate} exportable={false} style={{ minWidth: '12rem' }} />
            </DataTable>

            {/* Aday Ekleme/Düzenleme Dialog */}
            <Dialog
                visible={adayDialog}
                style={{ width: '1000px', height: '80vh' }}
                header={aday.id ? "Aday Düzenle" : "Yeni Aday Ekle"}
                modal
                className="p-fluid"
                footer={adayDialogFooter}
                onHide={hideDialog}
            >
                <div>
                    <TabView scrollable>
                        <TabPanel header="Temel Bilgiler" leftIcon="pi pi-user ">
                            <div className="formgrid grid">
                                <div className="field col">
                                    <label htmlFor="ad">Ad *</label>
                                    <InputText
                                        id="ad"
                                        value={aday.ad || ''}
                                        onChange={(e) => onInputChange(e, 'ad')}
                                        required
                                        className={validationErrors.ad ? 'p-invalid' : ''}
                                    />
                                    {validationErrors.ad && <small className="p-error">{validationErrors.ad}</small>}
                                </div>
                                <div className="field col">
                                    <label htmlFor="soyad">Soyad *</label>
                                    <InputText
                                        id="soyad"
                                        value={aday.soyad || ''}
                                        onChange={(e) => onInputChange(e, 'soyad')}
                                        required
                                        className={validationErrors.soyad ? 'p-invalid' : ''}
                                    />
                                    {validationErrors.soyad && <small className="p-error">{validationErrors.soyad}</small>}
                                </div>
                            </div>

                            <div className="formgrid grid">
                                <div className="field col">
                                    <label htmlFor="tcKimlik">TC Kimlik *</label>
                                    <InputText
                                        id="tcKimlik"
                                        value={aday.tcKimlik || ''}
                                        onChange={(e) => onInputChange(e, 'tcKimlik')}
                                        required
                                        className={validationErrors.tcKimlik ? 'p-invalid' : ''}
                                    />
                                    {validationErrors.tcKimlik && <small className="p-error">{validationErrors.tcKimlik}</small>}
                                </div>
                                <div className="field col">
                                    <label htmlFor="email">Email *</label>
                                    <InputText
                                        id="email"
                                        value={aday.email || ''}
                                        onChange={(e) => onInputChange(e, 'email')}
                                        required
                                        className={validationErrors.email ? 'p-invalid' : ''}
                                    />
                                    {validationErrors.email && <small className="p-error">{validationErrors.email}</small>}
                                </div>
                            </div>

                            <div className="formgrid grid">
                                <div className="field col">
                                    <label htmlFor="telefon">Telefon</label>
                                    <InputText
                                        id="telefon"
                                        value={aday.telefon || ''}
                                        onChange={(e) => onInputChange(e, 'telefon')}
                                        className={validationErrors.telefon ? 'p-invalid' : ''}
                                        placeholder="05xxxxxxxxx"
                                    />
                                    {validationErrors.telefon && <small className="p-error">{validationErrors.telefon}</small>}
                                </div>
                                <div className="field col">
                                    <label htmlFor="dogumTarihi">Doğum Tarihi</label>
                                    <Calendar
                                        id="dogumTarihi"
                                        value={aday.dogumTarihi}
                                        onChange={(e) => onDateChange(e, 'dogumTarihi')}
                                        showIcon
                                        dateFormat="dd/mm/yy"
                                        locale="tr"
                                    />
                                </div>
                            </div>

                            <div className="formgrid grid">
                                <div className="field col">
                                    <label htmlFor="cinsiyet">Cinsiyet</label>
                                    <Dropdown
                                        id="cinsiyet"
                                        value={aday.cinsiyet}
                                        options={iseAlimService.getCinsiyetler()}
                                        onChange={(e) => onDropdownChange(e, 'cinsiyet')}
                                        optionLabel="label"
                                        optionValue="value"
                                        placeholder="Cinsiyet seçin"
                                    />
                                </div>
                                <div className="field col">
                                    <label htmlFor="medeniDurum">Medeni Durum</label>
                                    <Dropdown
                                        id="medeniDurum"
                                        value={aday.medeniDurum}
                                        options={iseAlimService.getMedeniDurumlar()}
                                        onChange={(e) => onDropdownChange(e, 'medeniDurum')}
                                        optionLabel="label"
                                        optionValue="value"
                                        placeholder="Medeni durum seçin"
                                    />
                                </div>
                            </div>

                            <div className="field">
                                <label htmlFor="adres">Adres</label>
                                <InputTextarea
                                    id="adres"
                                    value={aday.adres || ''}
                                    onChange={(e) => onInputChange(e, 'adres')}
                                    rows={2}
                                />
                            </div>

                            <div className="formgrid grid">
                                <div className="field col">
                                    <label htmlFor="sehir">Şehir</label>
                                    <Dropdown
                                        id="sehir"
                                        value={aday.sehir || ''}
                                        options={sehirler}
                                        onChange={(e) => onInputChange(e, 'sehir')}
                                        placeholder="Şehir seçiniz"
                                        showClear
                                        filter
                                    />
                                </div>
                                <div className="field col">
                                    <label htmlFor="linkedinUrl">LinkedIn URL</label>
                                    <InputText
                                        id="linkedinUrl"
                                        value={aday.linkedinUrl || ''}
                                        onChange={(e) => onInputChange(e, 'linkedinUrl')}
                                        className={validationErrors.linkedinUrl ? 'p-invalid' : ''}
                                        placeholder="https://www.linkedin.com/in/kullanici-adi"
                                    />
                                    {validationErrors.linkedinUrl && <small className="p-error">{validationErrors.linkedinUrl}</small>}
                                </div>
                            </div>

                            {/* Fotoğraf Yükleme */}
                            <div className="field">
                                <label htmlFor="fotografYukle">Profil Fotoğrafı</label>
                                <div className="formgrid grid">
                                    <div className="field col-12 md:col-8">
                                        <FileUpload
                                            name="fotografFile"
                                            accept="image/*"
                                            maxFileSize={5000000}
                                            mode="basic"
                                            auto={false}
                                            chooseLabel="Fotoğraf Seç"
                                            onSelect={onFileSelect}
                                            onClear={() => setAday(prev => ({...prev, fotografYolu: ''}))}
                                            customUpload={true}
                                            uploadHandler={customUploader}
                                        />
                                        <small className="text-muted">Desteklenen formatlar: JPG, PNG, GIF (Max: 5MB)</small>
                                    </div>
                                    <div className="field col-12 md:col-4">
                                        {(aday.fotografYolu || aday.fotografPreview) ? (
                                            <div className="text-center">
                                                <img
                                                    src={aday.fotografPreview || iseAlimService.getImageUrl(aday.fotografYolu)}
                                                    alt="Profil Fotoğrafı"
                                                    style={{width: '80px', height: '80px', objectFit: 'cover', borderRadius: '50%'}}
                                                />
                                                <div>
                                                    {aday.selectedFile && !aday.fotografYolu && (
                                                        <Button
                                                            label="Yükle"
                                                            icon="pi pi-upload"
                                                            className="p-button-success p-button-sm mt-2 mr-2"
                                                            onClick={customUploader}
                                                        />
                                                    )}
                                                    <Button
                                                        icon="pi pi-trash"
                                                        className="p-button-danger p-button-sm mt-2"
                                                        onClick={() => removeFotograf()}
                                                        tooltip="Fotoğrafı Sil"
                                                    />
                                                </div>
                                            </div>
                                        ) : (
                                            <div className="text-center text-muted">
                                                <i className="pi pi-image" style={{fontSize: '2rem'}}></i>
                                                <div>Fotoğraf yok</div>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </div>

                            <div className="field">
                                <label htmlFor="notlar">Notlar</label>
                                <InputTextarea
                                    id="notlar"
                                    value={aday.notlar || ''}
                                    onChange={(e) => onInputChange(e, 'notlar')}
                                    rows={3}
                                />
                            </div>
                        </TabPanel>

                        <TabPanel header="Eğitim Bilgileri" leftIcon="pi pi-graduation-cap ">
                            {aday.egitimler && aday.egitimler.map((egitim, index) => (
                                <Panel key={index} header={`Eğitim ${index + 1}`} className="mb-3">
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`okulAd_${index}`}>Okul/Üniversite Adı *</label>
                                            <InputText
                                                id={`okulAd_${index}`}
                                                value={egitim.okulAd || ''}
                                                onChange={(e) => updateEgitim(index, 'okulAd', e.target.value)}
                                                required
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`bolum_${index}`}>Bölüm *</label>
                                            <InputText
                                                id={`bolum_${index}`}
                                                value={egitim.bolum || ''}
                                                onChange={(e) => updateEgitim(index, 'bolum', e.target.value)}
                                                required
                                            />
                                        </div>
                                        <div className="field col-2">
                                            <label>&nbsp;</label>
                                            <Button
                                                icon="pi pi-trash"
                                                className="p-button-danger p-button-sm"
                                                onClick={() => removeEgitim(index)}
                                                tooltip="Sil"
                                            />
                                        </div>
                                    </div>
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`derece_${index}`}>Derece</label>
                                            <Dropdown
                                                id={`derece_${index}`}
                                                value={egitim.derece}
                                                options={[
                                                    { label: 'Lise', value: 'Lise' },
                                                    { label: 'Ön Lisans', value: 'Ön Lisans' },
                                                    { label: 'Lisans', value: 'Lisans' },
                                                    { label: 'Yüksek Lisans', value: 'Yüksek Lisans' },
                                                    { label: 'Doktora', value: 'Doktora' }
                                                ]}
                                                onChange={(e) => updateEgitim(index, 'derece', e.value)}
                                                placeholder="Derece seçin"
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`baslangicYili_${index}`}>Başlangıç Yılı *</label>
                                            <InputNumber
                                                id={`baslangicYili_${index}`}
                                                value={egitim.baslangicYili}
                                                onValueChange={(e) => updateEgitim(index, 'baslangicYili', e.value)}
                                                min={1980}
                                                max={new Date().getFullYear()}
                                                useGrouping={false}
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`mezuniyetYili_${index}`}>Mezuniyet Yılı</label>
                                            <InputNumber
                                                id={`mezuniyetYili_${index}`}
                                                value={egitim.mezuniyetYili}
                                                onValueChange={(e) => updateEgitim(index, 'mezuniyetYili', e.value)}
                                                min={1980}
                                                max={new Date().getFullYear() + 10}
                                                disabled={egitim.devamEdiyor}
                                                useGrouping={false}
                                            />
                                        </div>
                                        <div className="field col-3">
                                            <label>&nbsp;</label>
                                            <div className="field-checkbox">
                                                <input
                                                    type="checkbox"
                                                    id={`devamEdiyor_${index}`}
                                                    checked={egitim.devamEdiyor || false}
                                                    onChange={(e) => updateEgitim(index, 'devamEdiyor', e.target.checked)}
                                                />
                                                <label htmlFor={`devamEdiyor_${index}`}>Devam ediyor</label>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`notOrtalamasi_${index}`}>Not Ortalaması</label>
                                            <InputNumber
                                                id={`notOrtalamasi_${index}`}
                                                value={egitim.notOrtalamasi}
                                                onValueChange={(e) => updateEgitim(index, 'notOrtalamasi', e.value)}
                                                min={0}
                                                max={4}
                                                mode="decimal"
                                                minFractionDigits={2}
                                                maxFractionDigits={2}
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`aciklama_egitim_${index}`}>Açıklama</label>
                                            <InputTextarea
                                                id={`aciklama_egitim_${index}`}
                                                value={egitim.aciklama || ''}
                                                onChange={(e) => updateEgitim(index, 'aciklama', e.target.value)}
                                                rows={2}
                                            />
                                        </div>
                                    </div>
                                </Panel>
                            ))}
                            {(!aday.egitimler || aday.egitimler.length === 0) && (
                                <div className="text-center text-muted p-4">
                                    <i className="pi pi-graduation-cap" style={{ fontSize: '2rem', marginBottom: '1rem' }}></i>
                                    <div>Henüz eğitim eklenmemiş. "Eğitim Ekle" butonuna tıklayarak eğitim ekleyebilirsiniz.</div>
                                </div>
                            )}
                            <div className="mt-3">
                                <Button
                                    label="Eğitim Ekle"
                                    icon="pi pi-plus"
                                    className="p-button-sm p-button-outlined"
                                    onClick={() => addEgitim()}
                                />
                            </div>
                        </TabPanel>

                        <TabPanel header="İş Deneyimi" leftIcon="pi pi-briefcase ">
                            {aday.deneyimler && aday.deneyimler.map((deneyim, index) => (
                                <Panel key={index} header={`Deneyim ${index + 1}`} className="mb-3">
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`sirketAd_${index}`}>Şirket Adı *</label>
                                            <InputText
                                                id={`sirketAd_${index}`}
                                                value={deneyim.sirketAd || ''}
                                                onChange={(e) => updateDeneyim(index, 'sirketAd', e.target.value)}
                                                required
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`pozisyon_${index}`}>Pozisyon *</label>
                                            <InputText
                                                id={`pozisyon_${index}`}
                                                value={deneyim.pozisyon || ''}
                                                onChange={(e) => updateDeneyim(index, 'pozisyon', e.target.value)}
                                                required
                                            />
                                        </div>
                                        <div className="field col-2">
                                            <label>&nbsp;</label>
                                            <Button
                                                icon="pi pi-trash"
                                                className="p-button-danger p-button-sm"
                                                onClick={() => removeDeneyim(index)}
                                                tooltip="Sil"
                                            />
                                        </div>
                                    </div>
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`baslangicTarihi_${index}`}>Başlangıç Tarihi *</label>
                                            <Calendar
                                                id={`baslangicTarihi_${index}`}
                                                value={deneyim.baslangicTarihi ? new Date(deneyim.baslangicTarihi) : null}
                                                onChange={(e) => updateDeneyim(index, 'baslangicTarihi', e.value)}
                                                showIcon
                                                dateFormat="dd/mm/yy"
                                                locale="tr"
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`bitisTarihi_${index}`}>Bitiş Tarihi</label>
                                            <Calendar
                                                id={`bitisTarihi_${index}`}
                                                value={deneyim.bitisTarihi ? new Date(deneyim.bitisTarihi) : null}
                                                onChange={(e) => updateDeneyim(index, 'bitisTarihi', e.value)}
                                                showIcon
                                                dateFormat="dd/mm/yy"
                                                locale="tr"
                                                disabled={deneyim.halenCalisiyor}
                                            />
                                        </div>
                                        <div className="field col-3">
                                            <label>&nbsp;</label>
                                            <div className="field-checkbox">
                                                <input
                                                    type="checkbox"
                                                    id={`halenCalisiyor_${index}`}
                                                    checked={deneyim.halenCalisiyor || false}
                                                    onChange={(e) => updateDeneyim(index, 'halenCalisiyor', e.target.checked)}
                                                />
                                                <label htmlFor={`halenCalisiyor_${index}`}>Halen çalışıyor</label>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="field">
                                        <label htmlFor={`aciklama_${index}`}>Açıklama</label>
                                        <InputTextarea
                                            id={`aciklama_${index}`}
                                            value={deneyim.aciklama || ''}
                                            onChange={(e) => updateDeneyim(index, 'aciklama', e.target.value)}
                                            rows={2}
                                        />
                                    </div>
                                </Panel>
                            ))}
                            {(!aday.deneyimler || aday.deneyimler.length === 0) && (
                                <div className="text-center text-muted p-4">
                                    <i className="pi pi-briefcase" style={{ fontSize: '2rem', marginBottom: '1rem' }}></i>
                                    <div>Henüz deneyim eklenmemiş. "Deneyim Ekle" butonuna tıklayarak deneyim ekleyebilirsiniz.</div>
                                </div>
                            )}
                            <div className="mt-3">
                                <Button
                                    label="Deneyim Ekle"
                                    icon="pi pi-plus"
                                    className="p-button-sm p-button-outlined"
                                    onClick={() => addDeneyim()}
                                />
                            </div>
                        </TabPanel>

                        <TabPanel header="Yetenekler" leftIcon="pi pi-star ">
                            {aday.yetenekler && aday.yetenekler.map((yetenek, index) => (
                                <Panel key={index} header={`Yetenek ${index + 1}`} className="mb-3">
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`yetenek_${index}`}>Yetenek *</label>
                                            <InputText
                                                id={`yetenek_${index}`}
                                                value={yetenek.yetenek || ''}
                                                onChange={(e) => updateYetenek(index, 'yetenek', e.target.value)}
                                                required
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`seviye_${index}`}>Seviye *</label>
                                            <div className="flex align-items-center gap-2">
                                                <Rating
                                                    value={yetenek.seviye || 1}
                                                    onChange={(e) => updateYetenek(index, 'seviye', e.value)}
                                                    stars={5}
                                                    cancel={false}
                                                />
                                                <span className="text-sm text-muted">
                                                    {yetenek.seviye === 1 && "Başlangıç"}
                                                    {yetenek.seviye === 2 && "Temel"}
                                                    {yetenek.seviye === 3 && "Orta"}
                                                    {yetenek.seviye === 4 && "İyi"}
                                                    {yetenek.seviye === 5 && "Uzman"}
                                                </span>
                                            </div>
                                        </div>
                                        <div className="field col-2">
                                            <label>&nbsp;</label>
                                            <Button
                                                icon="pi pi-trash"
                                                className="p-button-danger p-button-sm"
                                                onClick={() => removeYetenek(index)}
                                                tooltip="Sil"
                                            />
                                        </div>
                                    </div>
                                </Panel>
                            ))}
                            {(!aday.yetenekler || aday.yetenekler.length === 0) && (
                                <div className="text-center text-muted p-4">
                                    <i className="pi pi-star" style={{ fontSize: '2rem', marginBottom: '1rem' }}></i>
                                    <div>Henüz yetenek eklenmemiş. "Yetenek Ekle" butonuna tıklayarak yetenek ekleyebilirsiniz.</div>
                                </div>
                            )}
                            <div className="mt-3">
                                <Button
                                    label="Yetenek Ekle"
                                    icon="pi pi-plus"
                                    className="p-button-sm p-button-outlined"
                                    onClick={() => addYetenek()}
                                />
                            </div>
                        </TabPanel>

                        <TabPanel header="Sertifikalar" leftIcon="pi pi-shield ">
                            {aday.sertifikalar && aday.sertifikalar.map((sertifika, index) => (
                                <Panel key={index} header={`Sertifika ${index + 1}`} className="mb-3">
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`sertifikaAd_${index}`}>Sertifika Adı *</label>
                                            <InputText
                                                id={`sertifikaAd_${index}`}
                                                value={sertifika.sertifikaAd || ''}
                                                onChange={(e) => updateSertifika(index, 'sertifikaAd', e.target.value)}
                                                required
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`verenKurum_${index}`}>Veren Kurum *</label>
                                            <InputText
                                                id={`verenKurum_${index}`}
                                                value={sertifika.verenKurum || ''}
                                                onChange={(e) => updateSertifika(index, 'verenKurum', e.target.value)}
                                                required
                                            />
                                        </div>
                                        <div className="field col-2">
                                            <label>&nbsp;</label>
                                            <Button
                                                icon="pi pi-trash"
                                                className="p-button-danger p-button-sm"
                                                onClick={() => removeSertifika(index)}
                                                tooltip="Sil"
                                            />
                                        </div>
                                    </div>
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`tarih_${index}`}>Tarih</label>
                                            <Calendar
                                                id={`tarih_${index}`}
                                                value={sertifika.tarih ? new Date(sertifika.tarih) : null}
                                                onChange={(e) => updateSertifika(index, 'tarih', e.value)}
                                                showIcon
                                                dateFormat="dd/mm/yy"
                                                locale="tr"
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`gecerlilikTarihi_${index}`}>Geçerlilik Tarihi</label>
                                            <Calendar
                                                id={`gecerlilikTarihi_${index}`}
                                                value={sertifika.gecerlilikTarihi ? new Date(sertifika.gecerlilikTarihi) : null}
                                                onChange={(e) => updateSertifika(index, 'gecerlilikTarihi', e.value)}
                                                showIcon
                                                dateFormat="dd/mm/yy"
                                                locale="tr"
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`sertifikaNo_${index}`}>Sertifika No</label>
                                            <InputText
                                                id={`sertifikaNo_${index}`}
                                                value={sertifika.sertifikaNo || ''}
                                                onChange={(e) => updateSertifika(index, 'sertifikaNo', e.target.value)}
                                            />
                                        </div>
                                    </div>
                                    <div className="field">
                                        <label htmlFor={`aciklama_sertifika_${index}`}>Açıklama</label>
                                        <InputTextarea
                                            id={`aciklama_sertifika_${index}`}
                                            value={sertifika.aciklama || ''}
                                            onChange={(e) => updateSertifika(index, 'aciklama', e.target.value)}
                                            rows={2}
                                        />
                                    </div>
                                </Panel>
                            ))}
                            {(!aday.sertifikalar || aday.sertifikalar.length === 0) && (
                                <div className="text-center text-muted p-4">
                                    <i className="pi pi-shield" style={{ fontSize: '2rem', marginBottom: '1rem' }}></i>
                                    <div>Henüz sertifika eklenmemiş. "Sertifika Ekle" butonuna tıklayarak sertifika ekleyebilirsiniz.</div>
                                </div>
                            )}
                            <div className="mt-3">
                                <Button
                                    label="Sertifika Ekle"
                                    icon="pi pi-plus"
                                    className="p-button-sm p-button-outlined"
                                    onClick={() => addSertifika()}
                                />
                            </div>
                        </TabPanel>

                        <TabPanel header="Diller" leftIcon="pi pi-globe ">
                            {aday.diller && aday.diller.map((dil, index) => (
                                <Panel key={index} header={`Dil ${index + 1}`} className="mb-3">
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`dil_${index}`}>Dil *</label>
                                            <InputText
                                                id={`dil_${index}`}
                                                value={dil.dil || ''}
                                                onChange={(e) => updateDil(index, 'dil', e.target.value)}
                                                required
                                            />
                                        </div>
                                        <div className="field col-2">
                                            <label>&nbsp;</label>
                                            <Button
                                                icon="pi pi-trash"
                                                className="p-button-danger p-button-sm"
                                                onClick={() => removeDil(index)}
                                                tooltip="Sil"
                                            />
                                        </div>
                                    </div>
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`okumaSeviyesi_${index}`}>Okuma Seviyesi</label>
                                            <div className="flex align-items-center gap-2">
                                                <Rating
                                                    value={dil.okumaSeviyesi || 1}
                                                    onChange={(e) => updateDil(index, 'okumaSeviyesi', e.value)}
                                                    stars={5}
                                                    cancel={false}
                                                />
                                                <span className="text-sm text-muted">Okuma</span>
                                            </div>
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`yazmaSeviyesi_${index}`}>Yazma Seviyesi</label>
                                            <div className="flex align-items-center gap-2">
                                                <Rating
                                                    value={dil.yazmaSeviyesi || 1}
                                                    onChange={(e) => updateDil(index, 'yazmaSeviyesi', e.value)}
                                                    stars={5}
                                                    cancel={false}
                                                />
                                                <span className="text-sm text-muted">Yazma</span>
                                            </div>
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`konusmaSeviyesi_${index}`}>Konuşma Seviyesi</label>
                                            <div className="flex align-items-center gap-2">
                                                <Rating
                                                    value={dil.konusmaSeviyesi || 1}
                                                    onChange={(e) => updateDil(index, 'konusmaSeviyesi', e.value)}
                                                    stars={5}
                                                    cancel={false}
                                                />
                                                <span className="text-sm text-muted">Konuşma</span>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`sertifika_${index}`}>Sertifika</label>
                                            <InputText
                                                id={`sertifika_${index}`}
                                                value={dil.sertifika || ''}
                                                onChange={(e) => updateDil(index, 'sertifika', e.target.value)}
                                                placeholder="TOEFL, IELTS, vs."
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`sertifikaPuani_${index}`}>Sertifika Puanı</label>
                                            <InputText
                                                id={`sertifikaPuani_${index}`}
                                                value={dil.sertifikaPuani || ''}
                                                onChange={(e) => updateDil(index, 'sertifikaPuani', e.target.value)}
                                            />
                                        </div>
                                    </div>
                                </Panel>
                            ))}
                            {(!aday.diller || aday.diller.length === 0) && (
                                <div className="text-center text-muted p-4">
                                    <i className="pi pi-globe" style={{ fontSize: '2rem', marginBottom: '1rem' }}></i>
                                    <div>Henüz dil eklenmemiş. "Dil Ekle" butonuna tıklayarak dil ekleyebilirsiniz.</div>
                                </div>
                            )}
                            <div className="mt-3">
                                <Button
                                    label="Dil Ekle"
                                    icon="pi pi-plus"
                                    className="p-button-sm p-button-outlined"
                                    onClick={() => addDil()}
                                />
                            </div>
                        </TabPanel>

                        <TabPanel header="Referanslar" leftIcon="pi pi-users ">
                            {aday.referanslar && aday.referanslar.map((referans, index) => (
                                <Panel key={index} header={`Referans ${index + 1}`} className="mb-3">
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`refAdSoyad_${index}`}>Ad Soyad *</label>
                                            <InputText
                                                id={`refAdSoyad_${index}`}
                                                value={referans.adSoyad || ''}
                                                onChange={(e) => updateReferans(index, 'adSoyad', e.target.value)}
                                                required
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`refSirket_${index}`}>Şirket</label>
                                            <InputText
                                                id={`refSirket_${index}`}
                                                value={referans.sirket || ''}
                                                onChange={(e) => updateReferans(index, 'sirket', e.target.value)}
                                            />
                                        </div>
                                        <div className="field col-2">
                                            <label>&nbsp;</label>
                                            <Button
                                                icon="pi pi-trash"
                                                className="p-button-danger p-button-sm"
                                                onClick={() => removeReferans(index)}
                                                tooltip="Sil"
                                            />
                                        </div>
                                    </div>
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`refPozisyon_${index}`}>Pozisyon</label>
                                            <InputText
                                                id={`refPozisyon_${index}`}
                                                value={referans.pozisyon || ''}
                                                onChange={(e) => updateReferans(index, 'pozisyon', e.target.value)}
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`refTelefon_${index}`}>Telefon</label>
                                            <InputText
                                                id={`refTelefon_${index}`}
                                                value={referans.telefon || ''}
                                                onChange={(e) => updateReferans(index, 'telefon', e.target.value)}
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`refEmail_${index}`}>Email</label>
                                            <InputText
                                                id={`refEmail_${index}`}
                                                value={referans.email || ''}
                                                onChange={(e) => updateReferans(index, 'email', e.target.value)}
                                            />
                                        </div>
                                    </div>
                                    <div className="formgrid grid">
                                        <div className="field col">
                                            <label htmlFor={`iliskiTuru_${index}`}>İlişki Türü</label>
                                            <InputText
                                                id={`iliskiTuru_${index}`}
                                                value={referans.iliskiTuru || ''}
                                                onChange={(e) => updateReferans(index, 'iliskiTuru', e.target.value)}
                                                placeholder="Eski Müdür, Meslektaş, vs."
                                            />
                                        </div>
                                        <div className="field col">
                                            <label htmlFor={`refAciklama_${index}`}>Açıklama</label>
                                            <InputTextarea
                                                id={`refAciklama_${index}`}
                                                value={referans.aciklama || ''}
                                                onChange={(e) => updateReferans(index, 'aciklama', e.target.value)}
                                                rows={2}
                                            />
                                        </div>
                                    </div>
                                </Panel>
                            ))}
                            {(!aday.referanslar || aday.referanslar.length === 0) && (
                                <div className="text-center text-muted p-4">
                                    <i className="pi pi-users" style={{ fontSize: '2rem', marginBottom: '1rem' }}></i>
                                    <div>Henüz referans eklenmemiş. "Referans Ekle" butonuna tıklayarak referans ekleyebilirsiniz.</div>
                                </div>
                            )}
                            <div className="mt-3">
                                <Button
                                    label="Referans Ekle"
                                    icon="pi pi-plus"
                                    className="p-button-sm p-button-outlined"
                                    onClick={() => addReferans()}
                                />
                            </div>
                        </TabPanel>
                    </TabView>
                </div>
            </Dialog>

            {/* Aday Detay Dialog - Modern Tasarım */}
            <Dialog
                visible={adayDetailDialog}
                style={{ width: '95vw', maxWidth: '1400px' }}
                header={
                    <div className="flex align-items-center gap-3">
                        <i className="pi pi-id-card text-3xl text-primary"></i>
                        <div>
                            <h3 className="m-0">{selectedAday ? `${selectedAday.ad} ${selectedAday.soyad}` : 'Aday Detayı'}</h3>
                            <small className="text-600">Özgeçmiş Detay Görüntüle</small>
                        </div>
                    </div>
                }
                modal
                className="p-fluid"
                onHide={hideDetailDialog}
                maximizable
                breakpoints={{ '960px': '100vw', '640px': '100vw' }}
            >
                {selectedAday && (
                    <div>
                        {/* Üst Kısım - Aday Özet Bilgileri */}
                        <div className="surface-card p-3 mb-3 border-round shadow-2">
                            <div className="grid align-items-center">
                                <div className="col-12 md:col-9">
                                    <div className="grid">
                                        <div className="col-12 md:col-4 mb-2">
                                            <div className="flex align-items-center gap-2">
                                                <i className="pi pi-envelope text-primary"></i>
                                                <div>
                                                    <small className="text-600">E-posta</small>
                                                    <div className="font-semibold">{selectedAday.email}</div>
                                                </div>
                                            </div>
                                        </div>
                                        <div className="col-12 md:col-4 mb-2">
                                            <div className="flex align-items-center gap-2">
                                                <i className="pi pi-phone text-primary"></i>
                                                <div>
                                                    <small className="text-600">Telefon</small>
                                                    <div className="font-semibold">{selectedAday.telefon}</div>
                                                </div>
                                            </div>
                                        </div>
                                        <div className="col-12 md:col-4 mb-2">
                                            <div className="flex align-items-center gap-2">
                                                <i className="pi pi-map-marker text-primary"></i>
                                                <div>
                                                    <small className="text-600">Konum</small>
                                                    <div className="font-semibold">{selectedAday.adres || 'Belirtilmemiş'}</div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div className="col-12 md:col-3 text-right">
                                    <Tag
                                        value={selectedAday.durum || 'Aktif'}
                                        severity={selectedAday.durum === 'Aktif' ? 'success' : 'warning'}
                                        className="mr-2"
                                    />
                                    {selectedAday.cvDosyaYolu && (
                                        <Button
                                            label="CV İndir"
                                            icon="pi pi-download"
                                            className="p-button-sm p-button-outlined"
                                            onClick={() => window.open(selectedAday.cvDosyaYolu, '_blank')}
                                        />
                                    )}
                                </div>
                            </div>
                        </div>

                        <TabView scrollable className="modern-tabview">
                            <TabPanel header="Genel Bilgiler" leftIcon="pi pi-user">
                                <div className="flex justify-content-center">
                                    <div className="col-12 lg:col-8">
                                        <Card className="border-round shadow-2">
                                            <h5 className="text-primary mb-3">
                                                <i className="pi pi-info-circle mr-2"></i>
                                                Kişisel Bilgiler
                                            </h5>
                                            <div className="grid">
                                                <div className="col-6 mb-3">
                                                    <label className="text-600 text-sm">Ad Soyad</label>
                                                    <div className="font-semibold">{selectedAday.adSoyad}</div>
                                                </div>
                                                <div className="col-6 mb-3">
                                                    <label className="text-600 text-sm">TC Kimlik No</label>
                                                    <div className="font-semibold">{selectedAday.tcKimlik || '-'}</div>
                                                </div>
                                                <div className="col-6 mb-3">
                                                    <label className="text-600 text-sm">Doğum Tarihi</label>
                                                    <div className="font-semibold">{selectedAday.dogumTarihi ? iseAlimService.formatDate(selectedAday.dogumTarihi) : '-'}</div>
                                                </div>
                                                <div className="col-6 mb-3">
                                                    <label className="text-600 text-sm">Cinsiyet</label>
                                                    <div className="font-semibold">{selectedAday.cinsiyet || '-'}</div>
                                                </div>
                                                <div className="col-6 mb-3">
                                                    <label className="text-600 text-sm">Medeni Durum</label>
                                                    <div className="font-semibold">{selectedAday.medeniDurum || '-'}</div>
                                                </div>
                                                <div className="col-6 mb-3">
                                                    <label className="text-600 text-sm">Askerlik Durumu</label>
                                                    <div className="font-semibold">{selectedAday.askerlikDurumu || '-'}</div>
                                                </div>
                                                <div className="col-6 mb-3">
                                                    <label className="text-600 text-sm">Üniversite</label>
                                                    <div className="font-semibold">{selectedAday.universite || '-'}</div>
                                                </div>
                                                <div className="col-6 mb-3">
                                                    <label className="text-600 text-sm">Bölüm</label>
                                                    <div className="font-semibold">{selectedAday.bolum || '-'}</div>
                                                </div>
                                                <div className="col-6 mb-3">
                                                    <label className="text-600 text-sm">Mezuniyet Yılı</label>
                                                    <div className="font-semibold">{selectedAday.mezuniyetYili || '-'}</div>
                                                </div>
                                                <div className="col-6 mb-3">
                                                    <label className="text-600 text-sm">Şehir</label>
                                                    <div className="font-semibold">{selectedAday.sehir || '-'}</div>
                                                </div>
                                                <div className="col-6 mb-3">
                                                    <label className="text-600 text-sm">Toplam Deneyim</label>
                                                    <div className="font-semibold">{selectedAday.deneyimYil || 'Yeni mezun'}</div>
                                                </div>
                                                <div className="col-6 mb-3">
                                                    <label className="text-600 text-sm">LinkedIn</label>
                                                    <div className="font-semibold">
                                                        {selectedAday.linkedinUrl ? (
                                                            <a href={selectedAday.linkedinUrl} target="_blank" rel="noopener noreferrer" className="text-primary">
                                                                <i className="pi pi-external-link mr-1"></i>
                                                                Profili Görüntüle
                                                            </a>
                                                        ) : '-'}
                                                    </div>
                                                </div>
                                            </div>
                                            {selectedAday.notlar && (
                                                <div className="mt-3 p-3 surface-100 border-round">
                                                    <div className="flex align-items-center gap-2 mb-2">
                                                        <i className="pi pi-comment text-primary"></i>
                                                        <small className="text-600 font-semibold">Notlar</small>
                                                    </div>
                                                    <div className="text-sm">{selectedAday.notlar}</div>
                                                </div>
                                            )}
                                        </Card>
                                    </div>
                                </div>
                            </TabPanel>

                            <TabPanel header="Deneyimler" leftIcon="pi pi-briefcase">
                                {selectedAday.deneyimler && selectedAday.deneyimler.length > 0 ? (
                                    <Timeline value={selectedAday.deneyimler} align="alternate" className="customized-timeline"
                                        marker={(item) => <span className="flex w-2rem h-2rem align-items-center justify-content-center text-white border-circle z-1 shadow-2" style={{ backgroundColor: '#6366F1' }}><i className="pi pi-briefcase text-sm"></i></span>}
                                        content={(item) => (
                                            <Card className="shadow-2">
                                                <div className="flex justify-content-between align-items-center mb-2">
                                                    <h5 className="text-primary m-0">{item.sirketAd}</h5>
                                                    <Tag value={item.halenCalisiyor ? 'Devam Ediyor' : 'Ayrıldı'} severity={item.halenCalisiyor ? 'success' : 'info'} />
                                                </div>
                                                <p className="font-semibold mb-1">{item.pozisyon}</p>
                                                <p className="text-600 text-sm mb-2">
                                                    <i className="pi pi-calendar mr-1"></i>
                                                    {item.sure}
                                                </p>
                                                {item.aciklama && (
                                                    <p className="text-sm line-height-3">{item.aciklama}</p>
                                                )}
                                            </Card>
                                        )}
                                    />
                                ) : (
                                    <div className="text-center text-muted p-4">
                                        <i className="pi pi-briefcase" style={{ fontSize: '3rem', marginBottom: '1rem' }}></i>
                                        <div>Deneyim bilgisi bulunmamaktadır.</div>
                                    </div>
                                )}
                            </TabPanel>

                            <TabPanel header="Eğitim" leftIcon="pi pi-graduation-cap">
                                {selectedAday.egitimler && selectedAday.egitimler.length > 0 ? (
                                    <div className="grid">
                                        {selectedAday.egitimler.map((egitim, index) => (
                                            <div key={index} className="col-12 md:col-6 mb-3">
                                                <Card className="h-full shadow-2 hover:shadow-4 transition-all transition-duration-300">
                                                    <div className="flex align-items-start gap-3">
                                                        <div className="flex align-items-center justify-content-center bg-blue-100 border-circle" style={{ width: '3rem', height: '3rem' }}>
                                                            <i className="pi pi-graduation-cap text-blue-600 text-xl"></i>
                                                        </div>
                                                        <div className="flex-1">
                                                            <h6 className="text-primary mb-2">{egitim.okulAd}</h6>
                                                            <p className="font-semibold text-900 mb-1">{egitim.bolum}</p>
                                                            <div className="flex flex-wrap gap-2 mb-2">
                                                                <Chip label={egitim.derece} className="text-xs" />
                                                                <Chip label={egitim.sure} className="text-xs" icon="pi pi-calendar" />
                                                            </div>
                                                            {egitim.notOrtalamasi && (
                                                                <div className="text-sm text-600 mb-1">
                                                                    <i className="pi pi-star-fill text-yellow-500 mr-1"></i>
                                                                    GNO: {egitim.notOrtalamasi}
                                                                </div>
                                                            )}
                                                            {egitim.aciklama && (
                                                                <p className="text-sm text-600 line-height-2 m-0">{egitim.aciklama}</p>
                                                            )}
                                                        </div>
                                                    </div>
                                                </Card>
                                            </div>
                                        ))}
                                    </div>
                                ) : (
                                    <div className="text-center text-muted p-4">
                                        <i className="pi pi-graduation-cap" style={{ fontSize: '3rem', marginBottom: '1rem' }}></i>
                                        <div>Eğitim bilgisi bulunmamaktadır.</div>
                                    </div>
                                )}
                            </TabPanel>

                            <TabPanel header="Yetenekler" leftIcon="pi pi-star">
                                {selectedAday.yetenekler && selectedAday.yetenekler.length > 0 ? (
                                    <div className="grid">
                                        {selectedAday.yetenekler.map((yetenek, index) => (
                                            <div key={index} className="col-12 md:col-6 lg:col-4 mb-3">
                                                <Card className="h-full shadow-2 hover:shadow-4 transition-all transition-duration-300">
                                                    <div className="flex flex-column gap-2">
                                                        <div className="flex justify-content-between align-items-center">
                                                            <span className="font-semibold text-900">{yetenek.yetenek}</span>
                                                            <Chip label={yetenek.seviyeText} className="text-xs" severity="info" />
                                                        </div>
                                                        <Rating value={yetenek.seviye} readOnly stars={5} cancel={false} className="text-sm" />
                                                    </div>
                                                </Card>
                                            </div>
                                        ))}
                                    </div>
                                ) : (
                                    <div className="text-center text-muted p-4">
                                        <i className="pi pi-star" style={{ fontSize: '3rem', marginBottom: '1rem' }}></i>
                                        <div>Yetenek bilgisi bulunmamaktadır.</div>
                                    </div>
                                )}
                            </TabPanel>

                            <TabPanel header="Diller" leftIcon="pi pi-globe">
                                {selectedAday.diller && selectedAday.diller.length > 0 ? (
                                    <div className="grid">
                                        {selectedAday.diller.map((dil, index) => (
                                            <div key={index} className="col-12 lg:col-6 mb-3">
                                                <Card className="h-full shadow-2">
                                                    <div className="flex align-items-start gap-3">
                                                        <div className="flex align-items-center justify-content-center bg-purple-100 border-circle" style={{ width: '3rem', height: '3rem' }}>
                                                            <i className="pi pi-globe text-purple-600 text-xl"></i>
                                                        </div>
                                                        <div className="flex-1">
                                                            <h6 className="text-primary mb-3">{dil.dil}</h6>
                                                            <div className="grid">
                                                                <div className="col-4">
                                                                    <div className="text-center">
                                                                        <small className="text-600 font-semibold">Okuma</small>
                                                                        <Rating value={dil.okumaSeviyesi} readOnly stars={5} cancel={false} className="text-xs" />
                                                                        <div className="text-xs text-600">{dil.okumaSeviyesiText}</div>
                                                                    </div>
                                                                </div>
                                                                <div className="col-4">
                                                                    <div className="text-center">
                                                                        <small className="text-600 font-semibold">Yazma</small>
                                                                        <Rating value={dil.yazmaSeviyesi} readOnly stars={5} cancel={false} className="text-xs" />
                                                                        <div className="text-xs text-600">{dil.yazmaSeviyesiText}</div>
                                                                    </div>
                                                                </div>
                                                                <div className="col-4">
                                                                    <div className="text-center">
                                                                        <small className="text-600 font-semibold">Konuşma</small>
                                                                        <Rating value={dil.konusmaSeviyesi} readOnly stars={5} cancel={false} className="text-xs" />
                                                                        <div className="text-xs text-600">{dil.konusmaSeviyesiText}</div>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            {dil.sertifika && (
                                                                <div className="mt-3 p-2 surface-100 border-round">
                                                                    <i className="pi pi-certificate mr-1 text-primary"></i>
                                                                    <span className="text-sm">{dil.sertifika}</span>
                                                                    {dil.sertifikaPuani && <Chip label={dil.sertifikaPuani} className="ml-2 text-xs" />}
                                                                </div>
                                                            )}
                                                        </div>
                                                    </div>
                                                </Card>
                                            </div>
                                        ))}
                                    </div>
                                ) : (
                                    <div className="text-center text-muted p-4">
                                        <i className="pi pi-globe" style={{ fontSize: '3rem', marginBottom: '1rem' }}></i>
                                        <div>Dil bilgisi bulunmamaktadır.</div>
                                    </div>
                                )}
                            </TabPanel>

                            <TabPanel header="Referanslar" leftIcon="pi pi-users">
                                {selectedAday.referanslar && selectedAday.referanslar.length > 0 ? (
                                    <div className="grid">
                                        {selectedAday.referanslar.map((referans, index) => (
                                            <div key={index} className="col-12 md:col-6 lg:col-4 mb-3">
                                                <Card className="h-full shadow-2">
                                                    <div className="flex align-items-start gap-3">
                                                        <div className="flex align-items-center justify-content-center bg-orange-100 border-circle" style={{ width: '2.5rem', height: '2.5rem' }}>
                                                            <i className="pi pi-user text-orange-600"></i>
                                                        </div>
                                                        <div className="flex-1">
                                                            <h6 className="text-900 mb-2">{referans.adSoyad}</h6>
                                                            <div className="text-sm text-600 mb-1">
                                                                <i className="pi pi-building mr-1 text-400"></i>
                                                                {referans.sirket} - {referans.pozisyon}
                                                            </div>
                                                            <div className="text-sm text-600 mb-1">
                                                                <i className="pi pi-tag mr-1 text-400"></i>
                                                                {referans.iliskiTuru}
                                                            </div>
                                                            <div className="flex gap-2 mt-2">
                                                                {referans.telefon && (
                                                                    <a href={`tel:${referans.telefon}`} className="text-primary text-sm">
                                                                        <i className="pi pi-phone"></i>
                                                                    </a>
                                                                )}
                                                                {referans.email && (
                                                                    <a href={`mailto:${referans.email}`} className="text-primary text-sm">
                                                                        <i className="pi pi-envelope"></i>
                                                                    </a>
                                                                )}
                                                            </div>
                                                        </div>
                                                    </div>
                                                </Card>
                                            </div>
                                        ))}
                                    </div>
                                ) : (
                                    <div className="text-center text-muted p-4">
                                        <i className="pi pi-users" style={{ fontSize: '3rem', marginBottom: '1rem' }}></i>
                                        <div>Referans bilgisi bulunmamaktadır.</div>
                                    </div>
                                )}
                            </TabPanel>
                        </TabView>
                    </div>
                )}
            </Dialog>


            <ConfirmDialog
                visible={deleteAdayDialog}
                onHide={hideDeleteAdayDialog}
                message="Bu adayı silmek istediğinizden emin misiniz?"
                header="Onay"
                icon="pi pi-exclamation-triangle"
                footer={deleteAdayDialogFooter}
            />

            {/* CV Yönetimi Dialog */}
            <Dialog
                visible={cvDialog}
                style={{ width: '800px' }}
                header={selectedAday ? `${selectedAday.adSoyad} - CV Yönetimi` : 'CV Yönetimi'}
                modal
                onHide={hideCvDialog}
                key={cvDialog ? 'cv-dialog-open' : 'cv-dialog-closed'}
                contentStyle={{ transform: 'translateZ(0)' }}
            >
                <div className="mb-4">
                    <div className="flex gap-2 mb-3">
                        <Button
                            label="Otomatik CV Oluştur"
                            icon="pi pi-cog"
                            className="p-button-info"
                            onClick={otomatikCVOlustur}
                            loading={cvOlusturuluyor}
                            disabled={cvOlusturuluyor}
                        />
                        <Button
                            label="CV Yükle"
                            icon="pi pi-upload"
                            className="p-button-success"
                            onClick={() => setCvYuklemeDialog(true)}
                        />
                    </div>

                    {cvOlusturuluyor && (
                        <div className="mb-3">
                            <ProgressBar mode="indeterminate" style={{ height: '6px' }} />
                            <small className="text-muted">CV oluşturuluyor...</small>
                        </div>
                    )}
                </div>

                <Divider />

                <div className="cv-list">
                    <h5>CV Listesi</h5>
                    {cvListesi && cvListesi.length > 0 ? (
                        <div className="grid">
                            {cvListesi.map((cv, index) => (
                                <div key={index} className="col-12 md:col-6">
                                    <Panel header={cv.cvTipi} className="mb-3">
                                        <div className="flex justify-content-between align-items-center">
                                            <div>
                                                <div><strong>Tip:</strong> {cv.cvTipi}</div>
                                                {cv.dosyaAdi && <div><strong>Dosya:</strong> {cv.dosyaAdi}</div>}
                                                <div><strong>Tarih:</strong> {iseAlimService.formatDateShort(cv.createdAt)}</div>
                                                {cv.dosyaBoyutu && (
                                                    <div><strong>Boyut:</strong> {(cv.dosyaBoyutu / 1024 / 1024).toFixed(2)} MB</div>
                                                )}
                                            </div>
                                            <div className="flex gap-1">
                                                <Button
                                                    icon="pi pi-eye"
                                                    className="p-button-rounded p-button-info p-button-sm"
                                                    onClick={() => cvGoruntule(cv)}
                                                    tooltip="Görüntüle"
                                                />
                                                <Button
                                                    icon="pi pi-trash"
                                                    className="p-button-rounded p-button-danger p-button-sm"
                                                    onClick={() => cvSil(cv)}
                                                    tooltip="Sil"
                                                />
                                            </div>
                                        </div>
                                    </Panel>
                                </div>
                            ))}
                        </div>
                    ) : (
                        <div className="text-center text-muted p-4">
                            <i className="pi pi-file" style={{ fontSize: '3rem', marginBottom: '1rem' }}></i>
                            <div>Henüz CV bulunmamaktadır.</div>
                            <div>Otomatik CV oluşturabilir veya manuel CV yükleyebilirsiniz.</div>
                        </div>
                    )}
                </div>
            </Dialog>

            {/* CV Yükleme Dialog */}
            <Dialog
                visible={cvYuklemeDialog}
                style={{ width: '600px' }}
                header="CV Yükle"
                modal
                onHide={hideCvYuklemeDialog}
                key={cvYuklemeDialog ? 'cv-upload-open' : 'cv-upload-closed'}
                contentStyle={{ transform: 'translateZ(0)' }}
            >
                <div className="text-center">
                    <FileUpload
                        ref={fileUploadRef}
                        mode="basic"
                        name="cv"
                        accept=".pdf,.doc,.docx"
                        maxFileSize={10000000}
                        customUpload
                        uploadHandler={cvYukle}
                        auto={false}
                        chooseLabel="CV Dosyası Seç"
                        disabled={cvYukleniyor}
                    />
                    <div className="mt-3">
                        <small className="text-muted">
                            Desteklenen formatlar: PDF, DOC, DOCX<br />
                            Maksimum dosya boyutu: 10MB
                        </small>
                    </div>

                    {cvYukleniyor && (
                        <div className="mt-3">
                            <ProgressBar mode="indeterminate" style={{ height: '6px' }} />
                            <small className="text-muted">CV yükleniyor...</small>
                        </div>
                    )}
                </div>
            </Dialog>

            {/* CV Görüntüleme Dialog */}
            <Dialog
                visible={cvGoruntuleDialog}
                style={{ width: '90vw', height: '90vh' }}
                header={
                    <div className="flex justify-content-between align-items-center w-full">
                        <span>{selectedCv ? `${selectedCv.cvTipi} CV` : 'CV Görüntüle'}</span>
                        {cvContent && (
                            <div className="flex gap-2">
                                <Button
                                    icon="pi pi-print"
                                    className="p-button-rounded p-button-text"
                                    onClick={handlePrintCV}
                                    tooltip="Yazdır"
                                />
                                <Button
                                    icon="pi pi-download"
                                    className="p-button-rounded p-button-text"
                                    onClick={handleDownloadPDF}
                                    tooltip="PDF İndir"
                                />
                            </div>
                        )}
                    </div>
                }
                modal
                maximizable
                onHide={hideCvGoruntuleDialog}
                key={cvGoruntuleDialog ? 'cv-view-open' : 'cv-view-closed'}
                contentStyle={{ transform: 'translateZ(0)' }}
            >
                {cvContent ? (
                    <div dangerouslySetInnerHTML={{ __html: cvContent }} />
                ) : (
                    <div className="text-center p-4">
                        <i className="pi pi-spin pi-spinner" style={{ fontSize: '2rem' }}></i>
                        <div className="mt-2">CV yükleniyor...</div>
                    </div>
                )}
            </Dialog>
        </div>
    );
};

export default OzgecmisHavuzu;