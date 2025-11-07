import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { InputTextarea } from 'primereact/inputtextarea';
import { Badge } from 'primereact/badge';
import { confirmDialog } from 'primereact/confirmdialog';
import { Card } from 'primereact/card';
import { Calendar } from 'primereact/calendar';
import { Chip } from 'primereact/chip';
import istifaService from '../services/istifaService';
import authService from '../services/authService';
import yetkiService from '../services/yetkiService';

const IstifaIslemleri = () => {
    const [istifaTalepleri, setIstifaTalepleri] = useState([]);
    const [istifaDialog, setIstifaDialog] = useState(false);
    const [printDialog, setPrintDialog] = useState(false);
    const [istifaTalebi, setIstifaTalebi] = useState({
        id: null,
        personelId: null,
        sonCalismaTarihi: null,
        istifaNedeni: ''
    });
    const [submitted, setSubmitted] = useState(false);
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
    const [currentUser, setCurrentUser] = useState(null);
    const [selectedIstifaForPrint, setSelectedIstifaForPrint] = useState(null);
    const [permissions, setPermissions] = useState({
        read: false,
        write: false,
        delete: false,
        update: false
    });
    
    const toast = useRef(null);
    const dt = useRef(null);

    useEffect(() => {
        const user = authService.getUser();
        setCurrentUser(user);
        loadData();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('istifa-islemleri', 'read'),
                write: yetkiService.hasScreenPermission('istifa-islemleri', 'write'),
                delete: yetkiService.hasScreenPermission('istifa-islemleri', 'delete'),
                update: yetkiService.hasScreenPermission('istifa-islemleri', 'update')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
            setPermissions({
                read: false,
                write: false,
                delete: false,
                update: false
            });
        }
    };

    const loadData = async () => {
        const user = authService.getUser();
        if (!user) return;

        setLoading(true);
        try {
            const response = await istifaService.getIstifaTalepleri(user.personel.id);
            if (response.success) {
                setIstifaTalepleri(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'İstifa talepleri yüklenirken bir hata oluştu.',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const openNew = () => {
        const user = authService.getUser();
        setIstifaTalebi({
            id: null,
            personelId: user.personel.id,
            sonCalismaTarihi: null,
            istifaNedeni: ''
        });
        setSubmitted(false);
        setIstifaDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setIstifaDialog(false);
    };

    const saveIstifaTalebi = async () => {
        setSubmitted(true);

        if (istifaTalebi.sonCalismaTarihi) {
            try {
                let response;
                if (istifaTalebi.id) {
                    response = await istifaService.updateIstifaTalebi(istifaTalebi.id, istifaTalebi);
                } else {
                    response = await istifaService.createIstifaTalebi(istifaTalebi);
                }

                if (response.success) {
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: response.message,
                        life: 3000
                    });
                    loadData();
                    setIstifaDialog(false);
                }
            } catch (error) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: error.message,
                    life: 3000
                });
            }
        }
    };

    const editIstifaTalebi = (istifa) => {
        setIstifaTalebi({
            ...istifa,
            sonCalismaTarihi: istifa.sonCalismaTarihi ? new Date(istifa.sonCalismaTarihi) : null
        });
        setIstifaDialog(true);
    };

    const confirmDeleteIstifa = (istifa) => {
        confirmDialog({
            message: 'Bu istifa talebini silmek istediğinizden emin misiniz?',
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deleteIstifaTalebi(istifa.id)
        });
    };

    const deleteIstifaTalebi = async (id) => {
        try {
            const response = await istifaService.deleteIstifaTalebi(id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadData();
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message,
                life: 3000
            });
        }
    };

    const openPrintDialog = (istifa) => {
        setSelectedIstifaForPrint(istifa);
        setPrintDialog(true);
    };

    const hidePrintDialog = () => {
        setPrintDialog(false);
        setSelectedIstifaForPrint(null);
    };

    const generateIstifaForm = () => {
        if (!selectedIstifaForPrint) {
            toast.current.show({ 
                severity: 'warn', 
                summary: 'Uyarı', 
                detail: 'İstifa talebi seçilmedi' 
            });
            return;
        }

        // Yeni pencere aç
        const newWindow = window.open('', '_blank');
        if (!newWindow) {
            toast.current.show({ 
                severity: 'error', 
                summary: 'Hata', 
                detail: 'Yeni pencere açılamadı' 
            });
            return;
        }

        const now = new Date();
        const tarihSaat = `${now.toLocaleDateString('tr-TR')} ${now.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}`;
        
        const formHtml = `
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8">
            <title>İstifa Dilekçesi</title>
            <style>
                body { font-family: Arial, sans-serif; margin: 40px; font-size: 14px; line-height: 1.6; }
                .header { text-align: center; margin-bottom: 40px; }
                .company-title { font-size: 18px; font-weight: bold; margin-bottom: 5px; }
                .company-address { font-size: 12px; margin-bottom: 30px; }
                .form-title { font-size: 16px; font-weight: bold; margin-bottom: 30px; text-decoration: underline; text-align: center; }
                
                .recipient { margin-bottom: 30px; }
                .content { margin-bottom: 40px; text-align: justify; }
                .signature-section { margin-top: 50px; text-align: right; }
                .signature-line { display: inline-block; border-bottom: 1px solid #000; width: 200px; margin-bottom: 5px; }
                
                @media print {
                    body { margin: 0; padding: 20px; }
                    .no-print { display: none !important; }
                }
            </style>
        </head>
        <body>
            <div class="header">
                <div class="company-title">Icon A.Ş.</div>
                <div class="company-address">İnsan Kaynakları Müdürlüğü'ne</div>
                <div class="form-title">İSTİFA DİLEKÇESİ</div>
            </div>

            <div class="recipient">
                <strong>Sayın Yetkili,</strong>
            </div>

            <div class="content">
                <p>
                    ${istifaService.formatTarih(currentUser?.personel?.iseBaslamaTarihi)} tarihinden bu yana 
                    ${currentUser?.personel?.pozisyon?.departman?.ad || 'Departmanım'} departmanında 
                    ${currentUser?.personel?.pozisyon?.ad || 'pozisyonumda'} görev yapmaktayım.
                </p>
                
                <p>
                    ${selectedIstifaForPrint.istifaNedeni || 'Kişisel nedenlerimden dolayı'} 
                    ${istifaService.formatTarih(selectedIstifaForPrint.sonCalismaTarihi)} tarihinde işten ayrılmak istiyorum.
                </p>
                
                <p>
                    Bu güne kadar bana verdiğiniz destek ve gösterdiğiniz anlayış için teşekkür ederim.
                </p>
            </div>

            <div class="signature-section">
                <p><strong>Saygılarımla,</strong></p>
                <br><br>
                <div class="signature-line"></div>
                <p>
                    <strong>${currentUser?.personel?.ad || ''} ${currentUser?.personel?.soyad || ''}</strong><br>
                    TC Kimlik No: ${currentUser?.personel?.tcKimlik || currentUser?.personel?.TcKimlik || '______________'}<br>
                    Telefon: ${currentUser?.personel?.telefon || currentUser?.personel?.Telefon || '______________'}<br>
                    E-posta: ${currentUser?.personel?.email || currentUser?.personel?.Email || '______________'}
                </p>
            </div>

            <script>
                window.onload = function() {
                    window.print();
                };
            </script>
        </body>
        </html>
        `;

        newWindow.document.write(formHtml);
        newWindow.document.close();
        
        hidePrintDialog();
        toast.current.show({ 
            severity: 'success', 
            summary: 'Başarılı', 
            detail: 'İstifa dilekçesi hazırlandı' 
        });
    };

    const downloadDilekce = async (istifa) => {
        try {
            await istifaService.getIstifaDilekcesi(istifa.id);
            toast.current.show({
                severity: 'success',
                summary: 'Başarılı',
                detail: 'İstifa dilekçesi indiriliyor...',
                life: 3000
            });
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Dilekçe indirilemedi.',
                life: 3000
            });
        }
    };

    const onInputChange = (e, name) => {
        const val = (e.target && e.target.value) || e.value || '';
        let _istifaTalebi = { ...istifaTalebi };
        _istifaTalebi[`${name}`] = val;
        setIstifaTalebi(_istifaTalebi);
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                {permissions.write && (
                    <Button
                        label="Yeni İstifa Talebi"
                        icon="pi pi-plus"
                        className="p-button-success p-mr-2"
                        onClick={openNew}
                    />
                )}
            </React.Fragment>
        );
    };

    const rightToolbarTemplate = () => {
        return null;
    };

    const actionBodyTemplate = (rowData) => {
        const canEdit = rowData.onayDurumu === 'Beklemede' && permissions.update;
        const canDelete = rowData.onayDurumu === 'Beklemede' && permissions.delete;
        const canPrint = permissions.read; // Tüm kayıtlar için yazıcı ikonu göster

        return (
            <React.Fragment>
                {canEdit && (
                    <Button
                        icon="pi pi-pencil"
                        className="p-button-rounded p-button-success p-mr-2"
                        onClick={() => editIstifaTalebi(rowData)}
                        tooltip="Düzenle"
                    />
                )}
                {canDelete && (
                    <Button
                        icon="pi pi-trash"
                        className="p-button-rounded p-button-warning p-mr-2"
                        onClick={() => confirmDeleteIstifa(rowData)}
                        tooltip="Sil"
                    />
                )}
                {canPrint && (
                    <Button
                        icon="pi pi-print"
                        className="p-button-rounded p-button-info"
                        onClick={() => openPrintDialog(rowData)}
                        tooltip="Yazdır"
                    />
                )}
            </React.Fragment>
        );
    };

    const durumBodyTemplate = (rowData) => {
        const getDurumSeverity = (durum) => {
            switch (durum) {
                case 'Onaylandı': return 'success';
                case 'Reddedildi': return 'danger';
                case 'Beklemede': return 'warning';
                default: return 'info';
            }
        };

        return <Badge value={rowData.onayDurumu} severity={getDurumSeverity(rowData.onayDurumu)}></Badge>;
    };

    const tarihBodyTemplate = (rowData) => {
        return istifaService.formatTarih(rowData.istifaTarihi);
    };

    const sonCalismaTarihiBodyTemplate = (rowData) => {
        return istifaService.formatTarih(rowData.sonCalismaTarihi);
    };

    const header = (
        <div className="flex flex-column md:flex-row md:justify-content-between md:align-items-center">
            <h5 className="m-0">İstifa Talepleri</h5>
            <span className="block mt-2 md:mt-0 p-input-icon-left">
                <i className="pi pi-search" />
                <InputText type="search" onInput={(e) => setGlobalFilter(e.target.value)} placeholder="Ara..." />
            </span>
        </div>
    );

    const istifaDialogFooter = (
        <React.Fragment>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label="Kaydet" icon="pi pi-check" className="p-button-text" onClick={saveIstifaTalebi} />
        </React.Fragment>
    );

    if (!permissions.read) {
        return (
            <div className="surface-card p-4 shadow-2 border-round">
                <div className="text-center">
                    <i className="pi pi-lock text-6xl text-red-500 mb-3"></i>
                    <div className="text-xl font-medium text-900 mb-2">Yetkisiz Erişim</div>
                    <div className="text-600 mb-5">Bu sayfayı görüntüleme yetkiniz bulunmamaktadır.</div>
                </div>
            </div>
        );
    }

    return (
        <div className="grid">
            <div className="col-12">
                <Toast ref={toast} />

                {/* Bilgilendirme Kartı */}
                {currentUser && currentUser.personel && (
                    <Card className="mb-4">
                        <div className="flex justify-content-between align-items-center">
                            <h6>Personel Bilgileriniz</h6>
                            <div className="flex align-items-center gap-2">
                                <Chip 
                                    label={`İşe Başlama: ${istifaService.formatTarih(currentUser.personel.iseBaslamaTarihi || currentUser.personel.IseBaslamaTarihi)}`} 
                                    className="p-mr-2" 
                                />
                                <Chip 
                                    label={`Çalışma Yılı: ${istifaService.hesaplaCalismaYili(currentUser.personel.iseBaslamaTarihi || currentUser.personel.IseBaslamaTarihi)} yıl`} 
                                    className="p-chip-success" 
                                />
                            </div>
                        </div>
                    </Card>
                )}

                <Card>
                    <Toolbar
                        className="mb-4"
                        left={leftToolbarTemplate}
                        right={rightToolbarTemplate}
                    ></Toolbar>

                    <DataTable
                        ref={dt}
                        value={istifaTalepleri}
                        dataKey="id"
                        paginator
                        rows={10}
                        rowsPerPageOptions={[5, 10, 25]}
                        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                        currentPageReportTemplate="{first} - {last} arası, toplam {totalRecords} kayıt"
                        globalFilter={globalFilter}
                        header={header}
                        responsiveLayout="scroll"
                        loading={loading}
                        emptyMessage="İstifa talebi bulunamadı."
                    >
                        <Column
                            field="id"
                            header="ID"
                            sortable
                            style={{ minWidth: '4rem', width: '4rem' }}
                        ></Column>
                        <Column
                            field="istifaTarihi"
                            header="İstifa Tarihi"
                            body={tarihBodyTemplate}
                            sortable
                            style={{ minWidth: '10rem' }}
                        ></Column>
                        <Column
                            field="sonCalismaTarihi"
                            header="Son Çalışma Tarihi"
                            body={sonCalismaTarihiBodyTemplate}
                            sortable
                            style={{ minWidth: '12rem' }}
                        ></Column>
                        <Column
                            field="istifaNedeni"
                            header="İstifa Nedeni"
                            sortable
                            style={{ minWidth: '15rem' }}
                        ></Column>
                        <Column
                            field="onayDurumu"
                            header="Durum"
                            body={durumBodyTemplate}
                            sortable
                            style={{ minWidth: '8rem' }}
                        ></Column>
                        <Column
                            field="onaylayanAd"
                            header="Onaylayan"
                            sortable
                            style={{ minWidth: '12rem' }}
                        ></Column>
                        <Column
                            field="onayNotu"
                            header="Onay Notu"
                            sortable
                            style={{ minWidth: '15rem' }}
                        ></Column>
                        <Column
                            body={actionBodyTemplate}
                            header="İşlemler"
                            style={{ minWidth: '10rem' }}
                        ></Column>
                    </DataTable>
                </Card>

                {/* İstifa Talebi Dialog */}
                <Dialog
                    visible={istifaDialog}
                    style={{ width: '450px' }}
                    header="İstifa Talebi Detayları"
                    modal
                    className="p-fluid"
                    footer={istifaDialogFooter}
                    onHide={hideDialog}
                >
                    <div className="field">
                        <label htmlFor="sonCalismaTarihi">Son Çalışma Tarihi *</label>
                        <Calendar
                            id="sonCalismaTarihi"
                            value={istifaTalebi.sonCalismaTarihi}
                            onChange={(e) => onInputChange(e, 'sonCalismaTarihi')}
                            dateFormat="dd.mm.yy"
                            locale="tr"
                            showIcon
                            className={submitted && !istifaTalebi.sonCalismaTarihi ? 'p-invalid' : ''}
                            minDate={new Date()}
                        />
                        {submitted && !istifaTalebi.sonCalismaTarihi && (
                            <small className="p-error">Son çalışma tarihi gereklidir.</small>
                        )}
                    </div>

                    <div className="field">
                        <label htmlFor="istifaNedeni">İstifa Nedeni</label>
                        <InputTextarea
                            id="istifaNedeni"
                            value={istifaTalebi.istifaNedeni}
                            onChange={(e) => onInputChange(e, 'istifaNedeni')}
                            rows={4}
                            cols={20}
                            placeholder="İstifa nedeninizi belirtiniz (isteğe bağlı)"
                        />
                        <small className="p-text-secondary">
                            Boş bırakılırsa &quot;Kişisel nedenlerimden dolayı&quot; olarak geçecektir.
                        </small>
                    </div>
                </Dialog>

                {/* Print Dialog */}
                <Dialog
                    visible={printDialog}
                    style={{ width: '400px' }}
                    header="İstifa Dilekçesi Yazdır"
                    modal
                    className="p-fluid"
                    footer={
                        <React.Fragment>
                            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hidePrintDialog} />
                            <Button label="Yazdır" icon="pi pi-print" className="p-button-text" onClick={generateIstifaForm} />
                        </React.Fragment>
                    }
                    onHide={hidePrintDialog}
                >
                    {selectedIstifaForPrint && (
                        <>
                            <div className="field">
                                <label><strong>Personel:</strong> {currentUser?.personel?.ad || ''} {currentUser?.personel?.soyad || ''}</label>
                            </div>
                            <div className="field">
                                <label><strong>İstifa Tarihi:</strong> {istifaService.formatTarih(selectedIstifaForPrint.istifaTarihi)}</label>
                            </div>
                            <div className="field">
                                <label><strong>Son Çalışma Tarihi:</strong> {istifaService.formatTarih(selectedIstifaForPrint.sonCalismaTarihi)}</label>
                            </div>
                            <div className="field">
                                <label><strong>İstifa Nedeni:</strong> {selectedIstifaForPrint.istifaNedeni || 'Kişisel nedenlerimden dolayı'}</label>
                            </div>
                            <div className="field">
                                <label><strong>Durum:</strong> {selectedIstifaForPrint.onayDurumu}</label>
                            </div>
                            <div className="p-message p-message-info">
                                <div className="p-message-wrapper">
                                    <span className="p-message-icon pi pi-info-circle"></span>
                                    <span className="p-message-text">
                                        İstifa dilekçeniz otomatik olarak hazırlanıp yazdırma penceresinde açılacaktır.
                                    </span>
                                </div>
                            </div>
                        </>
                    )}
                </Dialog>
            </div>
        </div>
    );
};

export default IstifaIslemleri;