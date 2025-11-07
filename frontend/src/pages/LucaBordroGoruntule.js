import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { Toast } from 'primereact/toast';
import { Tag } from 'primereact/tag';
import { Card } from 'primereact/card';
import lucaBordroService from '../services/lucaBordroService';
import OtpDialog from '../components/OtpDialog';

const LucaBordroGoruntule = () => {
    const [bordrolar, setBordrolar] = useState([]);
    const [loading, setLoading] = useState(false);
    const [detayDialogVisible, setDetayDialogVisible] = useState(false);
    const [otpDialogVisible, setOtpDialogVisible] = useState(false);
    const [selectedBordro, setSelectedBordro] = useState(null);
    const [otpLoading, setOtpLoading] = useState(false);
    const toast = useRef(null);

    useEffect(() => {
        loadBordrolar();
    }, []);

    const loadBordrolar = async () => {
        setLoading(true);
        try {
            const response = await lucaBordroService.getBenimBordrolarim();
            if (response.success) {
                setBordrolar(response.data || []);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Bordrolar yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const showDetay = (bordro) => {
        setSelectedBordro(bordro);
        setDetayDialogVisible(true);
    };

    const hideDetayDialog = () => {
        setDetayDialogVisible(false);
        setSelectedBordro(null);
    };

    const requestOtp = async (bordro) => {
        setSelectedBordro(bordro);
        setOtpLoading(true);
        try {
            const response = await lucaBordroService.otpGonder(bordro.id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message || 'OTP kodu e-posta adresinize gönderildi',
                    life: 3000
                });
                setOtpDialogVisible(true);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'OTP gönderimi başarısız',
                life: 3000
            });
        } finally {
            setOtpLoading(false);
        }
    };

    const handleOtpSubmit = async (otpKodu) => {
        setOtpLoading(true);
        try {
            const response = await lucaBordroService.otpDogrulaVeGonder(selectedBordro.id, otpKodu);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message || 'Bordronuz e-posta adresinize gönderildi',
                    life: 3000
                });
                setOtpDialogVisible(false);
                setSelectedBordro(null);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'OTP doğrulama başarısız',
                life: 3000
            });
        } finally {
            setOtpLoading(false);
        }
    };

    const formatCurrency = (value) => {
        if (!value) return '0,00 ₺';
        return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);
    };

    const formatDate = (value) => {
        if (!value) return '-';
        return new Date(value).toLocaleDateString('tr-TR');
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-2">
                <Button
                    icon="pi pi-eye"
                    className="p-button-rounded p-button-info"
                    onClick={() => showDetay(rowData)}
                    tooltip="Detay"
                    tooltipOptions={{ position: 'top' }}
                />
                <Button
                    icon="pi pi-envelope"
                    className="p-button-rounded p-button-success"
                    onClick={() => requestOtp(rowData)}
                    loading={otpLoading && selectedBordro?.id === rowData.id}
                    tooltip="Mail'e Gönder"
                    tooltipOptions={{ position: 'top' }}
                />
            </div>
        );
    };

    const donemBodyTemplate = (rowData) => {
        const aylar = ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
            'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'];
        return `${aylar[rowData.donemAy - 1]} ${rowData.donemYil}`;
    };

    const durumBodyTemplate = (rowData) => {
        return (
            <Tag
                value={rowData.durum}
                severity={rowData.durum === 'Aktif' ? 'success' : 'info'}
            />
        );
    };

    const brutMaasBodyTemplate = (rowData) => {
        return formatCurrency(rowData.brutMaas);
    };

    const netUcretBodyTemplate = (rowData) => {
        return formatCurrency(rowData.netUcret);
    };

    const detayDialogFooter = (
        <div>
            <Button
                label="Kapat"
                icon="pi pi-times"
                className="p-button-text"
                onClick={hideDetayDialog}
            />
            <Button
                label="Mail'e Gönder"
                icon="pi pi-envelope"
                onClick={() => {
                    hideDetayDialog();
                    requestOtp(selectedBordro);
                }}
            />
        </div>
    );

    return (
        <div className="grid">
            <div className="col-12">
                <div className="card">
                    <Toast ref={toast} />
                    <h5>Luca Bordro Görüntüleme</h5>
                    <p className="text-600 mb-4">
                        Bu sayfada Luca programından alınan bordrolarınızı görüntüleyebilir ve e-posta adresinize gönderebilirsiniz.
                    </p>

                    <DataTable
                        value={bordrolar}
                        loading={loading}
                        responsiveLayout="scroll"
                        emptyMessage="Bordro bulunamadı"
                        paginator
                        rows={10}
                        rowsPerPageOptions={[10, 20, 50]}
                    >
                        <Column field="bordroNo" header="Bordro No" sortable />
                        <Column field="donemYil" header="Dönem" body={donemBodyTemplate} sortable />
                        <Column field="brutMaas" header="Brüt Maaş" body={brutMaasBodyTemplate} sortable />
                        <Column field="netUcret" header="Net Ücret" body={netUcretBodyTemplate} sortable />
                        <Column field="durum" header="Durum" body={durumBodyTemplate} sortable />
                        <Column field="senkronTarihi" header="Senkron Tarihi" body={(data) => formatDate(data.senkronTarihi)} sortable />
                        <Column body={actionBodyTemplate} exportable={false} style={{ minWidth: '10rem' }} />
                    </DataTable>

                    {/* Detay Dialog */}
                    <Dialog
                        visible={detayDialogVisible}
                        style={{ width: '700px' }}
                        header="Bordro Detayı"
                        modal
                        footer={detayDialogFooter}
                        onHide={hideDetayDialog}
                    >
                        {selectedBordro && (
                            <div className="grid">
                                <div className="col-12">
                                    <Card title="Genel Bilgiler" className="mb-3">
                                        <div className="grid">
                                            <div className="col-6">
                                                <strong>Ad Soyad:</strong>
                                                <p>{selectedBordro.adSoyad}</p>
                                            </div>
                                            <div className="col-6">
                                                <strong>TC Kimlik:</strong>
                                                <p>{selectedBordro.tcKimlik}</p>
                                            </div>
                                            <div className="col-6">
                                                <strong>Sicil No:</strong>
                                                <p>{selectedBordro.sicilNo || '-'}</p>
                                            </div>
                                            <div className="col-6">
                                                <strong>Bordro No:</strong>
                                                <p>{selectedBordro.bordroNo || '-'}</p>
                                            </div>
                                            <div className="col-6">
                                                <strong>Dönem:</strong>
                                                <p>{donemBodyTemplate(selectedBordro)}</p>
                                            </div>
                                            <div className="col-6">
                                                <strong>Durum:</strong>
                                                <p>{durumBodyTemplate(selectedBordro)}</p>
                                            </div>
                                        </div>
                                    </Card>
                                </div>

                                <div className="col-12">
                                    <Card title="Ücret Bilgileri" className="mb-3">
                                        <div className="grid">
                                            <div className="col-6">
                                                <strong>Brüt Maaş:</strong>
                                                <p className="text-xl font-bold text-primary">{formatCurrency(selectedBordro.brutMaas)}</p>
                                            </div>
                                            <div className="col-6">
                                                <strong>Net Ücret:</strong>
                                                <p className="text-xl font-bold text-green-600">{formatCurrency(selectedBordro.netUcret)}</p>
                                            </div>
                                            <div className="col-6">
                                                <strong>Toplam Ödeme:</strong>
                                                <p>{formatCurrency(selectedBordro.toplamOdeme)}</p>
                                            </div>
                                            <div className="col-6">
                                                <strong>Toplam Kesinti:</strong>
                                                <p className="text-red-600">{formatCurrency(selectedBordro.toplamKesinti)}</p>
                                            </div>
                                        </div>
                                    </Card>
                                </div>

                                <div className="col-12">
                                    <Card title="Kesintiler">
                                        <div className="grid">
                                            <div className="col-4">
                                                <strong>SGK İşçi Payı:</strong>
                                                <p>{formatCurrency(selectedBordro.sgkIsci)}</p>
                                            </div>
                                            <div className="col-4">
                                                <strong>Gelir Vergisi:</strong>
                                                <p>{formatCurrency(selectedBordro.gelirVergisi)}</p>
                                            </div>
                                            <div className="col-4">
                                                <strong>Damga Vergisi:</strong>
                                                <p>{formatCurrency(selectedBordro.damgaVergisi)}</p>
                                            </div>
                                        </div>
                                    </Card>
                                </div>
                            </div>
                        )}
                    </Dialog>

                    {/* OTP Dialog */}
                    <OtpDialog
                        visible={otpDialogVisible}
                        onHide={() => {
                            setOtpDialogVisible(false);
                            setSelectedBordro(null);
                        }}
                        onSubmit={handleOtpSubmit}
                        loading={otpLoading}
                    />
                </div>
            </div>
        </div>
    );
};

export default LucaBordroGoruntule;
