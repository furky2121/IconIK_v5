import React, { useState, useEffect, useRef } from 'react';
import { TabView, TabPanel } from 'primereact/tabview';
import { InputText } from 'primereact/inputtext';
import { Button } from 'primereact/button';
import { Password } from 'primereact/password';
import { Checkbox } from 'primereact/checkbox';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Dialog } from 'primereact/dialog';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { Dropdown } from 'primereact/dropdown';
import { Calendar } from 'primereact/calendar';
import { InputTextarea } from 'primereact/inputtextarea';
import { Tag } from 'primereact/tag';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import ePostaService from '../services/ePostaService';

const EPostaAyarlari = () => {
    const toast = useRef(null);
    const [activeIndex, setActiveIndex] = useState(0);
    const [loading, setLoading] = useState(false);
    const [testingConnection, setTestingConnection] = useState(false);

    // Error details state
    const [errorDialog, setErrorDialog] = useState(false);
    const [errorDetails, setErrorDetails] = useState({ message: '', details: '' });

    // SMTP Ayarları State
    const [smtpAyarlari, setSmtpAyarlari] = useState({
        id: null,
        smtpHost: '',
        smtpPort: 587,
        smtpUsername: '',
        smtpPassword: '',
        enableSsl: true,
        fromEmail: '',
        fromName: '',
        aktif: true
    });

    // Yönlendirme State
    const [yonlendirmeler, setYonlendirmeler] = useState([]);
    const [yonlendirmeTurleri, setYonlendirmeTurleri] = useState([]);
    const [yonlendirmeDialog, setYonlendirmeDialog] = useState(false);
    const [yonlendirme, setYonlendirme] = useState({
        id: null,
        yonlendirmeTuru: 'MulakatPlanlama',
        aliciEmail: '',
        gonderimSaati: null,
        aktif: true,
        aciklama: ''
    });
    const [isEdit, setIsEdit] = useState(false);

    useEffect(() => {
        loadSmtpAyarlari();
        loadYonlendirmeler();
        loadYonlendirmeTurleri();
    }, []);

    // ==================== SMTP AYARLARI ====================

    const loadSmtpAyarlari = async () => {
        setLoading(true);
        try {
            const result = await ePostaService.getSmtpAyarlari();
            if (result.success && result.data.length > 0) {
                // İlk (en son) kaydı al
                const ayar = result.data[0];
                setSmtpAyarlari({
                    ...ayar,
                    smtpPassword: '' // Şifreyi boş bırak, kullanıcı değiştirmek isterse girer
                });
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'SMTP ayarları yüklenemedi',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const saveSmtpAyarlari = async () => {
        // Validasyon
        if (!smtpAyarlari.smtpHost || !smtpAyarlari.smtpUsername || !smtpAyarlari.fromEmail) {
            toast.current.show({
                severity: 'warn',
                summary: 'Uyarı',
                detail: 'Lütfen zorunlu alanları doldurun',
                life: 3000
            });
            return;
        }

        if (!smtpAyarlari.id && !smtpAyarlari.smtpPassword) {
            toast.current.show({
                severity: 'warn',
                summary: 'Uyarı',
                detail: 'Şifre alanı zorunludur',
                life: 3000
            });
            return;
        }

        setLoading(true);
        try {
            const result = await ePostaService.saveSmtpAyarlari(smtpAyarlari);
            if (result.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: result.message,
                    life: 3000
                });
                await loadSmtpAyarlari();
            } else {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: result.message,
                    life: 3000
                });
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'SMTP ayarları kaydedilemedi',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const testConnection = async () => {
        setTestingConnection(true);
        try {
            const result = await ePostaService.testSmtpConnection();
            if (result.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: result.message || 'SMTP bağlantı testi başarılı!',
                    life: 5000
                });
            } else {
                // Show error with details
                setErrorDetails({
                    message: result.message || 'SMTP bağlantısı başarısız',
                    details: result.details || 'Detaylı hata bilgisi bulunamadı'
                });
                setErrorDialog(true);
            }
        } catch (error) {
            // Network or unexpected errors
            setErrorDetails({
                message: 'Bağlantı testi sırasında beklenmeyen bir hata oluştu',
                details: error.message || 'Lütfen internet bağlantınızı kontrol edin ve tekrar deneyin.'
            });
            setErrorDialog(true);
        } finally {
            setTestingConnection(false);
        }
    };

    // ==================== YÖNLENDİRME ====================

    const loadYonlendirmeler = async () => {
        try {
            const result = await ePostaService.getYonlendirmeler();
            if (result.success) {
                // GonderimSaati'ni Date objesine çevir
                const data = result.data.map(y => ({
                    ...y,
                    gonderimSaati: y.gonderimSaati ? parseTimeSpan(y.gonderimSaati) : null
                }));
                setYonlendirmeler(data);
            }
        } catch (error) {
            // console.error('Load yönlendirmeler error:', error);
        }
    };

    const loadYonlendirmeTurleri = async () => {
        try {
            const result = await ePostaService.getYonlendirmeTurleri();
            if (result.success) {
                setYonlendirmeTurleri(result.data);
            }
        } catch (error) {
            // console.error('Load türler error:', error);
        }
    };

    const parseTimeSpan = (timeSpan) => {
        // "09:00:00" formatından Date objesine çevir
        if (typeof timeSpan === 'string') {
            const parts = timeSpan.split(':');
            if (parts.length >= 2) {
                const date = new Date();
                date.setHours(parseInt(parts[0]), parseInt(parts[1]), 0, 0);
                return date;
            }
        }
        return null;
    };

    const formatTimeForApi = (date) => {
        if (!date) return null;
        const hours = date.getHours().toString().padStart(2, '0');
        const minutes = date.getMinutes().toString().padStart(2, '0');
        return `${hours}:${minutes}`;
    };

    const openNewYonlendirme = () => {
        setYonlendirme({
            id: null,
            yonlendirmeTuru: 'MulakatPlanlama',
            aliciEmail: '',
            gonderimSaati: null,
            aktif: true,
            aciklama: ''
        });
        setIsEdit(false);
        setYonlendirmeDialog(true);
    };

    const editYonlendirme = (rowData) => {
        setYonlendirme({ ...rowData });
        setIsEdit(true);
        setYonlendirmeDialog(true);
    };

    const hideDialog = () => {
        setYonlendirmeDialog(false);
    };

    const saveYonlendirme = async () => {
        // Validasyon
        if (!yonlendirme.aliciEmail || !yonlendirme.gonderimSaati) {
            toast.current.show({
                severity: 'warn',
                summary: 'Uyarı',
                detail: 'Lütfen tüm alanları doldurun',
                life: 3000
            });
            return;
        }

        setLoading(true);
        try {
            const dataToSave = {
                ...yonlendirme,
                gonderimSaati: formatTimeForApi(yonlendirme.gonderimSaati)
            };

            const result = await ePostaService.saveYonlendirme(dataToSave);
            if (result.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: result.message,
                    life: 3000
                });
                await loadYonlendirmeler();
                hideDialog();
            } else {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: result.message,
                    life: 3000
                });
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Yönlendirme kaydedilemedi',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const confirmDeleteYonlendirme = (rowData) => {
        confirmDialog({
            message: 'Bu yönlendirme kaydını silmek istediğinize emin misiniz?',
            header: 'Onay',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deleteYonlendirme(rowData.id),
            acceptLabel: 'Evet',
            rejectLabel: 'Hayır',
            acceptClassName: 'p-button-danger'
        });
    };

    const deleteYonlendirme = async (id) => {
        try {
            const result = await ePostaService.deleteYonlendirme(id);
            if (result.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: result.message,
                    life: 3000
                });
                await loadYonlendirmeler();
            } else {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: result.message,
                    life: 3000
                });
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Yönlendirme silinemedi',
                life: 3000
            });
        }
    };

    // ==================== RENDER ====================

    const leftToolbarTemplate = () => {
        return (
            <Button label="Yeni" icon="pi pi-plus" className="p-button-success mr-2" onClick={openNewYonlendirme} />
        );
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-2">
                <Button
                    icon="pi pi-pencil"
                    className="p-button-rounded p-button-success p-button-text"
                    onClick={() => editYonlendirme(rowData)}
                />
                <Button
                    icon="pi pi-trash"
                    className="p-button-rounded p-button-danger p-button-text"
                    onClick={() => confirmDeleteYonlendirme(rowData)}
                />
            </div>
        );
    };

    const aktifBodyTemplate = (rowData) => {
        return (
            <Tag value={rowData.aktif ? 'Aktif' : 'Pasif'} severity={rowData.aktif ? 'success' : 'warning'} />
        );
    };

    const saatBodyTemplate = (rowData) => {
        if (rowData.gonderimSaati) {
            return rowData.gonderimSaati.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
        }
        return '-';
    };

    const turBodyTemplate = (rowData) => {
        const tur = yonlendirmeTurleri.find(t => t.value === rowData.yonlendirmeTuru);
        return tur ? tur.label : rowData.yonlendirmeTuru;
    };

    const yonlendirmeDialogFooter = (
        <React.Fragment>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label="Kaydet" icon="pi pi-check" className="p-button-text" onClick={saveYonlendirme} loading={loading} />
        </React.Fragment>
    );

    return (
        <div className="grid">
            <div className="col-12">
                <div className="card">
                    <Toast ref={toast} />
                    <ConfirmDialog />

                    <h5>E-Posta Ayarları</h5>

                    <TabView activeIndex={activeIndex} onTabChange={(e) => setActiveIndex(e.index)}>
                        {/* SEKME 1: SMTP KONFİGÜRASYONU */}
                        <TabPanel header="SMTP Konfigürasyonu" leftIcon="pi pi-cog">
                            <div className="p-fluid formgrid grid">
                                <div className="field col-12 md:col-6">
                                    <label htmlFor="smtpHost">SMTP Host *</label>
                                    <InputText
                                        id="smtpHost"
                                        value={smtpAyarlari.smtpHost}
                                        onChange={(e) => setSmtpAyarlari({ ...smtpAyarlari, smtpHost: e.target.value })}
                                        placeholder="smtp.gmail.com"
                                    />
                                </div>

                                <div className="field col-12 md:col-6">
                                    <label htmlFor="smtpPort">SMTP Port *</label>
                                    <InputText
                                        id="smtpPort"
                                        value={smtpAyarlari.smtpPort}
                                        onChange={(e) => setSmtpAyarlari({ ...smtpAyarlari, smtpPort: parseInt(e.target.value) || 0 })}
                                        placeholder="587"
                                        type="number"
                                    />
                                    <small className="p-text-secondary">
                                        Port 587 (StartTLS) veya Port 25 önerilir. Port 465 için SSL/TLS'i kapatın.
                                    </small>
                                </div>

                                <div className="field col-12 md:col-6">
                                    <label htmlFor="smtpUsername">Kullanıcı Adı *</label>
                                    <InputText
                                        id="smtpUsername"
                                        value={smtpAyarlari.smtpUsername}
                                        onChange={(e) => setSmtpAyarlari({ ...smtpAyarlari, smtpUsername: e.target.value })}
                                        placeholder="user@example.com"
                                    />
                                </div>

                                <div className="field col-12 md:col-6">
                                    <label htmlFor="smtpPassword">Şifre {!smtpAyarlari.id && '*'}</label>
                                    <Password
                                        id="smtpPassword"
                                        value={smtpAyarlari.smtpPassword}
                                        onChange={(e) => setSmtpAyarlari({ ...smtpAyarlari, smtpPassword: e.target.value })}
                                        placeholder={smtpAyarlari.id ? "Değiştirmek için girin" : "Şifre"}
                                        toggleMask
                                        feedback={false}
                                    />
                                </div>

                                <div className="field col-12 md:col-6">
                                    <label htmlFor="fromEmail">Gönderen E-posta *</label>
                                    <InputText
                                        id="fromEmail"
                                        value={smtpAyarlari.fromEmail}
                                        onChange={(e) => setSmtpAyarlari({ ...smtpAyarlari, fromEmail: e.target.value })}
                                        placeholder="noreply@IconIK.com"
                                    />
                                </div>

                                <div className="field col-12 md:col-6">
                                    <label htmlFor="fromName">Gönderen İsim</label>
                                    <InputText
                                        id="fromName"
                                        value={smtpAyarlari.fromName}
                                        onChange={(e) => setSmtpAyarlari({ ...smtpAyarlari, fromName: e.target.value })}
                                        placeholder="Icon İK"
                                    />
                                </div>

                                <div className="field col-12">
                                    <div className="field-checkbox">
                                        <Checkbox
                                            inputId="enableSsl"
                                            checked={smtpAyarlari.enableSsl}
                                            onChange={(e) => setSmtpAyarlari({ ...smtpAyarlari, enableSsl: e.checked })}
                                        />
                                        <label htmlFor="enableSsl">SSL/TLS Etkin</label>
                                    </div>
                                </div>

                                <div className="field col-12">
                                    <div className="flex gap-2">
                                        <Button
                                            label="Kaydet"
                                            icon="pi pi-save"
                                            className="p-button-primary"
                                            onClick={saveSmtpAyarlari}
                                            loading={loading}
                                        />
                                        <Button
                                            label="Bağlantıyı Test Et"
                                            icon="pi pi-check-circle"
                                            className="p-button-help"
                                            onClick={testConnection}
                                            loading={testingConnection}
                                            disabled={!smtpAyarlari.id}
                                        />
                                    </div>
                                </div>
                            </div>
                        </TabPanel>

                        {/* SEKME 2: E-POSTA YÖNLENDİRME */}
                        <TabPanel header="E-Posta Yönlendirme" leftIcon="pi pi-send">
                            <Toolbar className="mb-4" left={leftToolbarTemplate} />

                            <DataTable
                                value={yonlendirmeler}
                                dataKey="id"
                                paginator
                                rows={10}
                                rowsPerPageOptions={[5, 10, 25]}
                                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                                currentPageReportTemplate="{first} - {last} / {totalRecords} kayıt"
                                emptyMessage="Yönlendirme kaydı bulunamadı"
                            >
                                <Column field="yonlendirmeTuru" header="Tür" body={turBodyTemplate} />
                                <Column field="aliciEmail" header="Alıcı E-posta" />
                                <Column field="gonderimSaati" header="Gönderim Saati" body={saatBodyTemplate} />
                                <Column field="aktif" header="Durum" body={aktifBodyTemplate} />
                                <Column field="aciklama" header="Açıklama" />
                                <Column body={actionBodyTemplate} header="İşlemler" style={{ width: '12rem' }} />
                            </DataTable>

                            {/* Yönlendirme Dialog */}
                            <Dialog
                                visible={yonlendirmeDialog}
                                style={{ width: '600px' }}
                                header={isEdit ? 'Yönlendirme Düzenle' : 'Yeni Yönlendirme'}
                                modal
                                className="p-fluid"
                                footer={yonlendirmeDialogFooter}
                                onHide={hideDialog}
                            >
                                <div className="field">
                                    <label htmlFor="yonlendirmeTuru">Yönlendirme Türü *</label>
                                    <Dropdown
                                        id="yonlendirmeTuru"
                                        value={yonlendirme.yonlendirmeTuru}
                                        options={yonlendirmeTurleri}
                                        onChange={(e) => setYonlendirme({ ...yonlendirme, yonlendirmeTuru: e.value })}
                                        optionLabel="label"
                                        optionValue="value"
                                        placeholder="Tür seçin"
                                    />
                                </div>

                                <div className="field">
                                    <label htmlFor="aliciEmail">Alıcı E-posta *</label>
                                    <InputText
                                        id="aliciEmail"
                                        value={yonlendirme.aliciEmail}
                                        onChange={(e) => setYonlendirme({ ...yonlendirme, aliciEmail: e.target.value })}
                                        placeholder="user@example.com"
                                    />
                                </div>

                                <div className="field">
                                    <label htmlFor="gonderimSaati">Gönderim Saati *</label>
                                    <Calendar
                                        id="gonderimSaati"
                                        value={yonlendirme.gonderimSaati}
                                        onChange={(e) => setYonlendirme({ ...yonlendirme, gonderimSaati: e.value })}
                                        timeOnly
                                        hourFormat="24"
                                        placeholder="Saat seçin (örn: 09:00)"
                                    />
                                </div>

                                <div className="field">
                                    <label htmlFor="aciklama">Açıklama</label>
                                    <InputTextarea
                                        id="aciklama"
                                        value={yonlendirme.aciklama}
                                        onChange={(e) => setYonlendirme({ ...yonlendirme, aciklama: e.target.value })}
                                        rows={3}
                                        placeholder="Opsiyonel açıklama"
                                    />
                                </div>

                                <div className="field-checkbox">
                                    <Checkbox
                                        inputId="aktif"
                                        checked={yonlendirme.aktif}
                                        onChange={(e) => setYonlendirme({ ...yonlendirme, aktif: e.checked })}
                                    />
                                    <label htmlFor="aktif">Aktif</label>
                                </div>
                            </Dialog>
                        </TabPanel>
                    </TabView>

                    {/* Error Details Dialog */}
                    <Dialog
                        visible={errorDialog}
                        style={{ width: '600px' }}
                        header="SMTP Bağlantı Hatası"
                        modal
                        onHide={() => setErrorDialog(false)}
                        footer={
                            <div className="flex justify-content-end">
                                <Button
                                    label="Kapat"
                                    icon="pi pi-times"
                                    onClick={() => setErrorDialog(false)}
                                    className="p-button-text"
                                />
                            </div>
                        }
                    >
                        <div className="flex flex-column gap-3">
                            <div className="flex align-items-start gap-2">
                                <i className="pi pi-exclamation-circle text-red-500 text-2xl mt-1"></i>
                                <div className="flex-1">
                                    <h4 className="m-0 mb-2">{errorDetails.message}</h4>
                                    <div className="bg-red-50 border-left-3 border-red-500 p-3 mb-3">
                                        <pre style={{
                                            whiteSpace: 'pre-wrap',
                                            wordWrap: 'break-word',
                                            fontFamily: 'monospace',
                                            fontSize: '0.875rem',
                                            margin: 0,
                                            maxHeight: '300px',
                                            overflow: 'auto'
                                        }}>
                                            {errorDetails.details}
                                        </pre>
                                    </div>
                                </div>
                            </div>

                            <div className="bg-blue-50 border-left-3 border-blue-500 p-3">
                                <h5 className="mt-0 mb-2 text-blue-700">Yaygın SMTP Sağlayıcılar için Ayarlar:</h5>
                                <div className="text-sm">
                                    <div className="mb-2">
                                        <strong>Gmail:</strong> smtp.gmail.com, Port 587, SSL/TLS Açık
                                    </div>
                                    <div className="mb-2">
                                        <strong>Outlook/Hotmail:</strong> smtp.office365.com, Port 587, SSL/TLS Açık
                                    </div>
                                    <div className="mb-2">
                                        <strong>Natro (mail.kurumsaleposta.com):</strong> Port 587, SSL/TLS Açık (Önerilen)
                                    </div>
                                    <div>
                                        <strong>Not:</strong> Port 465 kullanıyorsanız SSL/TLS seçeneğini kapatmayı deneyin.
                                    </div>
                                </div>
                            </div>
                        </div>
                    </Dialog>
                </div>
            </div>
        </div>
    );
};

export default EPostaAyarlari;
