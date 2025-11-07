import React, { useState, useEffect } from 'react';
import { Dialog } from 'primereact/dialog';
import { Button } from 'primereact/button';
import { ScrollPanel } from 'primereact/scrollpanel';
import kvkkService from '../services/kvkkService';

const KVKKOnayDialog = ({ visible, onAccept, onReject }) => {
    const [kvkkMetin, setKvkkMetin] = useState(null);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (visible) {
            loadKVKKMetin();
        }
    }, [visible]);

    const loadKVKKMetin = async () => {
        setLoading(true);
        try {
            const response = await kvkkService.getAktif();
            if (response.success && response.data) {
                setKvkkMetin(response.data);
            } else {
                // Eğer aktif KVKK metni yoksa varsayılan metin göster
                setKvkkMetin({
                    baslik: 'KVKK Aydınlatma Metni',
                    metin: '<p>KVKK aydınlatma metni henüz tanımlanmamıştır.</p>'
                });
            }
        } catch (error) {
            // console.error('KVKK metni yüklenirken hata:', error);
            setKvkkMetin({
                baslik: 'KVKK Aydınlatma Metni',
                metin: '<p>KVKK aydınlatma metni yüklenirken bir hata oluştu.</p>'
            });
        } finally {
            setLoading(false);
        }
    };

    const handleAccept = () => {
        if (onAccept) {
            onAccept();
        }
    };

    const handleReject = () => {
        if (onReject) {
            onReject();
        }
    };

    const footer = (
        <div className="flex justify-content-between">
            <Button
                label="Reddet"
                icon="pi pi-times"
                onClick={handleReject}
                className="p-button-danger"
                disabled={loading}
            />
            <Button
                label="Onaylıyorum"
                icon="pi pi-check"
                onClick={handleAccept}
                className="p-button-success"
                disabled={loading}
            />
        </div>
    );

    return (
        <Dialog
            visible={visible}
            style={{ width: '50vw' }}
            breakpoints={{ '960px': '75vw', '641px': '90vw' }}
            header={kvkkMetin?.baslik || 'KVKK Aydınlatma Metni'}
            modal
            closable={false}
            footer={footer}
        >
            {loading ? (
                <div className="flex align-items-center justify-content-center" style={{ minHeight: '300px' }}>
                    <i className="pi pi-spin pi-spinner" style={{ fontSize: '2rem' }}></i>
                </div>
            ) : (
                <>
                    <div className="mb-3">
                        <p className="text-600">
                            Sisteme giriş yapabilmek için lütfen KVKK aydınlatma metnini okuyup onaylayınız.
                        </p>
                    </div>
                    <ScrollPanel style={{ width: '100%', height: '400px' }}>
                        <div className="p-3" style={{ whiteSpace: 'pre-wrap' }}>
                            {kvkkMetin?.metin || ''}
                        </div>
                    </ScrollPanel>
                    <div className="mt-3 p-3 bg-yellow-50 border-round">
                        <p className="text-sm text-600 m-0">
                            <i className="pi pi-info-circle mr-2"></i>
                            KVKK aydınlatma metnini reddederseniz sisteme giriş yapamazsınız.
                        </p>
                    </div>
                </>
            )}
        </Dialog>
    );
};

export default KVKKOnayDialog;
