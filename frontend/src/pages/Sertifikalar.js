import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Toast } from 'primereact/toast';
import { Button } from 'primereact/button';
import { Toolbar } from 'primereact/toolbar';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Tag } from 'primereact/tag';
import { Card } from 'primereact/card';
import { Divider } from 'primereact/divider';
import jsPDF from 'jspdf';
import 'jspdf/dist/jspdf.es.min.js';
import html2canvas from 'html2canvas';
import videoEgitimService from '../services/videoEgitimService';

const Sertifikalar = () => {
    const [sertifikalar, setSertifikalar] = useState([]);
    const [loading, setLoading] = useState(true);
    const [sertifikaDialog, setSertifikaDialog] = useState(false);
    const [selectedSertifika, setSelectedSertifika] = useState(null);
    const [globalFilter, setGlobalFilter] = useState('');
    const toast = useRef(null);
    const dt = useRef(null);
    const sertifikaCardRef = useRef(null);

    useEffect(() => {
        loadSertifikalar();
    }, []);

    const loadSertifikalar = async () => {
        try {
            setLoading(true);
            const response = await videoEgitimService.getSertifikalar();
            if (response.success) {
                setSertifikalar(response.data || []);
            } else {
                setSertifikalar([]);
                toast.current?.show({
                    severity: 'warn',
                    summary: 'Uyarı',
                    detail: 'Henüz sertifika bulunamadı'
                });
            }
        } catch (error) {
            // console.error('Sertifika yükleme hatası:', error);
            setSertifikalar([]);
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Sertifikalar yüklenirken hata oluştu'
            });
        } finally {
            setLoading(false);
        }
    };

    const viewSertifika = (sertifika) => {
        setSelectedSertifika(sertifika);
        setSertifikaDialog(true);
    };

    const hideSertifikaDialog = () => {
        setSertifikaDialog(false);
        setSelectedSertifika(null);
    };

    const downloadPDF = async () => {
        if (!selectedSertifika) return;

        try {
            toast.current?.show({
                severity: 'info',
                summary: 'Bilgi',
                detail: 'PDF hazırlanıyor, lütfen bekleyin...'
            });

            // Hide print/pdf buttons temporarily for cleaner capture
            const originalButtons = document.querySelector('.p-dialog-footer');
            if (originalButtons) {
                originalButtons.style.display = 'none';
            }

            // Try to get element by ID first, then by ref
            let element = document.getElementById('sertifika-card');
            if (!element && sertifikaCardRef.current) {
                element = sertifikaCardRef.current;
            }

            if (!element) {
                throw new Error('Sertifika elementi bulunamadı');
            }

            // Temporarily remove border for clean capture
            const originalBorder = element.style.border;
            element.style.border = 'none';

            // Wait a bit for DOM to stabilize
            await new Promise(resolve => setTimeout(resolve, 300));

            // Capture the certificate card as canvas with high quality
            const canvas = await html2canvas(element, {
                allowTaint: false,
                useCORS: false,
                scale: 2.0, // Higher scale for better quality
                backgroundColor: '#1e3a8a',
                logging: false,
                removeContainer: true,
                foreignObjectRendering: false,
                width: element.offsetWidth,
                height: element.offsetHeight,
                x: 0,
                y: 0,
                scrollX: 0,
                scrollY: 0
            });

            // Restore border and buttons
            element.style.border = originalBorder;
            if (originalButtons) {
                originalButtons.style.display = 'flex';
            }

            // Convert canvas to PDF
            const imgData = canvas.toDataURL('image/png', 0.95);
            const doc = new jsPDF({
                orientation: 'portrait',
                unit: 'mm',
                format: 'a4'
            });

            // Calculate dimensions to fit the certificate properly on A4 page
            const pdfWidth = doc.internal.pageSize.getWidth();  // ~210mm
            const pdfHeight = doc.internal.pageSize.getHeight(); // ~297mm
            const imgAspectRatio = canvas.width / canvas.height;

            // Use minimal margins for maximum page utilization
            const margin = 5; // 5mm margin for better fit
            const maxWidth = pdfWidth - (2 * margin);   // ~200mm
            const maxHeight = pdfHeight - (2 * margin); // ~287mm

            let finalWidth, finalHeight;

            // Calculate size to maximize page usage while maintaining aspect ratio
            if (imgAspectRatio > 1) {
                // Landscape-oriented content - fit to width
                finalWidth = maxWidth;
                finalHeight = finalWidth / imgAspectRatio;

                // If height exceeds page, scale down
                if (finalHeight > maxHeight) {
                    finalHeight = maxHeight;
                    finalWidth = finalHeight * imgAspectRatio;
                }
            } else {
                // Portrait-oriented content - fit to height
                finalHeight = maxHeight;
                finalWidth = finalHeight * imgAspectRatio;

                // If width exceeds page, scale down
                if (finalWidth > maxWidth) {
                    finalWidth = maxWidth;
                    finalHeight = finalWidth / imgAspectRatio;
                }
            }

            // Center the image on the page
            const x = (pdfWidth - finalWidth) / 2;
            const y = (pdfHeight - finalHeight) / 2;

            // Add image to PDF
            doc.addImage(imgData, 'PNG', x, y, finalWidth, finalHeight);

            // Save the PDF
            const fileName = `Sertifika_${selectedSertifika.sertifikaNo || 'unknown'}.pdf`;
            doc.save(fileName);

            toast.current?.show({
                severity: 'success',
                summary: 'Başarılı',
                detail: 'Sertifika PDF olarak indirildi'
            });
        } catch (error) {
            // console.error('PDF oluşturma hatası:', error);
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'PDF oluşturulurken hata oluştu: ' + error.message
            });
        }
    };

    const printSertifika = () => {
        window.print();
        toast.current?.show({
            severity: 'success',
            summary: 'Başarılı',
            detail: 'Sertifika yazdırılıyor...'
        });
    };

    const exportCSV = () => {
        dt.current?.exportCSV();
    };

    const rightToolbarTemplate = () => {
        return (
            <React.Fragment>
                <Button label="Export" icon="pi pi-upload" className="p-button-help" onClick={exportCSV} />
            </React.Fragment>
        );
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <Button
                icon="pi pi-eye"
                rounded
                outlined
                onClick={() => viewSertifika(rowData)}
                tooltip="Görüntüle"
            />
        );
    };

    const durumBodyTemplate = (rowData) => {
        const getSeverity = (durum) => {
            switch (durum) {
                case 'Geçerli': return 'success';
                case 'Aktif': return 'success';
                case 'Süresi Dolmuş': return 'danger';
                case 'İptal': return 'warning';
                default: return 'info';
            }
        };
        return <Tag value={rowData.durum || 'Bilinmiyor'} severity={getSeverity(rowData.durum)} />;
    };

    const dateBodyTemplate = (rowData, field) => {
        const date = rowData[field];
        if (!date) return '-';

        const dateObj = new Date(date);
        const formatted = dateObj.toLocaleDateString('tr-TR');

        // Geçerlilik tarihi kontrolü
        if (field === 'gecerlilikTarihi') {
            const today = new Date();
            const diffTime = dateObj.getTime() - today.getTime();
            const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

            if (diffDays < 0) {
                return <span className="text-red-500">{formatted} (Süresi dolmuş)</span>;
            } else if (diffDays < 30) {
                return <span className="text-orange-500">{formatted} ({diffDays} gün kaldı)</span>;
            }
        }

        return formatted;
    };

    const header = (
        <div className="flex flex-wrap gap-2 align-items-center justify-content-between">
            <h4 className="m-0">Sertifikalarım</h4>
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

    return (
        <div className="grid">
            <div className="col-12">
                <div className="card">
                    <Toast ref={toast} />
                    <Toolbar className="mb-4" right={rightToolbarTemplate} />

                    <DataTable
                        ref={dt}
                        value={sertifikalar}
                        dataKey="id"
                        loading={loading}
                        paginator
                        rows={10}
                        rowsPerPageOptions={[5, 10, 25]}
                        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                        currentPageReportTemplate="{first} - {last} / {totalRecords} kayıt"
                        globalFilter={globalFilter}
                        header={header}
                        responsiveLayout="scroll"
                        emptyMessage="Henüz sertifika bulunamadı."
                    >
                        <Column field="sertifikaNo" header="Sertifika No" sortable style={{ minWidth: '10rem' }} />
                        <Column field="personelAd" header="Personel" sortable style={{ minWidth: '12rem' }} />
                        <Column field="egitimAd" header="Eğitim" sortable style={{ minWidth: '12rem' }} />
                        <Column field="verilisTarihi" header="Veriliş Tarihi" body={(rowData) => dateBodyTemplate(rowData, 'verilisTarihi')} sortable style={{ minWidth: '10rem' }} />
                        <Column field="gecerlilikTarihi" header="Geçerlilik Tarihi" body={(rowData) => dateBodyTemplate(rowData, 'gecerlilikTarihi')} sortable style={{ minWidth: '12rem' }} />
                        <Column field="durum" header="Durum" body={durumBodyTemplate} sortable style={{ minWidth: '8rem' }} />
                        <Column body={actionBodyTemplate} exportable={false} style={{ minWidth: '8rem' }} />
                    </DataTable>

                    <Dialog
                        visible={sertifikaDialog}
                        style={{ width: '700px' }}
                        header="Sertifika Detayı"
                        modal
                        className="p-fluid"
                        footer={
                            <div className="flex justify-content-end gap-2">
                                <Button label="Yazdır" icon="pi pi-print" onClick={printSertifika} />
                                <Button label="PDF İndir" icon="pi pi-download" severity="info" onClick={downloadPDF} />
                                <Button label="Kapat" icon="pi pi-times" outlined onClick={hideSertifikaDialog} />
                            </div>
                        }
                        onHide={hideSertifikaDialog}
                    >
                        {selectedSertifika && (
                            <Card ref={sertifikaCardRef} id="sertifika-card" className="sertifika-card" style={{
                                background: 'linear-gradient(135deg, #1e3a8a 0%, #1e40af 100%)',
                                color: 'white',
                                border: '3px solid #d97706',
                                borderRadius: '12px'
                            }}>
                                <div className="text-center mb-4">
                                    <div style={{
                                        width: '60px',
                                        height: '60px',
                                        backgroundColor: '#d97706',
                                        borderRadius: '50%',
                                        margin: '0 auto 20px',
                                        display: 'flex',
                                        alignItems: 'center',
                                        justifyContent: 'center'
                                    }}>
                                        <i className="pi pi-star" style={{ fontSize: '24px', color: 'white' }}></i>
                                    </div>
                                    <h2 className="text-4xl font-bold mb-2" style={{ color: '#f59e0b' }}>SERTİFİKA</h2>
                                    <p className="text-xl">IconIK Eğitim Merkezi</p>
                                </div>

                                <Divider style={{ borderColor: '#d97706' }} />

                                <div className="text-center mb-4">
                                    <p className="text-lg mb-2">Bu belge</p>
                                    <h3 className="text-2xl font-bold mb-2" style={{ color: '#fbbf24' }}>
                                        {selectedSertifika.personelAd || 'N/A'}
                                    </h3>
                                    <p className="text-lg">kişisinin</p>
                                </div>

                                <div className="text-center mb-4">
                                    <h4 className="text-xl font-bold mb-2" style={{ color: '#fbbf24' }}>
                                        {selectedSertifika.egitimAd || 'N/A'}
                                    </h4>
                                    <p className="text-lg">video eğitimini başarıyla tamamladığını gösterir.</p>
                                </div>

                                <Divider style={{ borderColor: '#d97706' }} />

                                <div className="grid">
                                    <div className="col-6">
                                        <p className="mb-1"><strong>Sertifika No:</strong></p>
                                        <p>{selectedSertifika.sertifikaNo || 'N/A'}</p>
                                    </div>
                                    <div className="col-6">
                                        <p className="mb-1"><strong>Veriliş Tarihi:</strong></p>
                                        <p>{selectedSertifika.verilisTarihi ? new Date(selectedSertifika.verilisTarihi).toLocaleDateString('tr-TR') : '-'}</p>
                                    </div>
                                    <div className="col-6">
                                        <p className="mb-1"><strong>Geçerlilik Tarihi:</strong></p>
                                        <p>{selectedSertifika.gecerlilikTarihi ? new Date(selectedSertifika.gecerlilikTarihi).toLocaleDateString('tr-TR') : 'Süresiz'}</p>
                                    </div>
                                    <div className="col-6">
                                        <p className="mb-1"><strong>Durum:</strong></p>
                                        <p>{selectedSertifika.durum || 'Bilinmiyor'}</p>
                                    </div>
                                </div>

                                <div className="text-center mt-4">
                                    <div className="flex justify-content-around">
                                        <div>
                                            <div style={{ borderTop: '2px solid #d97706', width: '150px', marginTop: '50px' }}></div>
                                            <p className="mt-2">Eğitim Koordinatörü</p>
                                        </div>
                                        <div>
                                            <div style={{ borderTop: '2px solid #d97706', width: '150px', marginTop: '50px' }}></div>
                                            <p className="mt-2">Genel Müdür</p>
                                        </div>
                                    </div>
                                </div>
                            </Card>
                        )}
                    </Dialog>
                </div>
            </div>
        </div>
    );
};

export default Sertifikalar;