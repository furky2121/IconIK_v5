import React, { useState, useEffect } from 'react';
import { Dialog } from 'primereact/dialog';
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';

const OtpDialog = ({ visible, onHide, onSubmit, loading }) => {
    const [otpKodu, setOtpKodu] = useState('');
    const [countdown, setCountdown] = useState(300); // 5 dakika = 300 saniye

    useEffect(() => {
        if (visible) {
            setOtpKodu('');
            setCountdown(300);
        }
    }, [visible]);

    useEffect(() => {
        if (visible && countdown > 0) {
            const timer = setTimeout(() => {
                setCountdown(countdown - 1);
            }, 1000);

            return () => clearTimeout(timer);
        }
    }, [visible, countdown]);

    const formatTime = (seconds) => {
        const minutes = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${minutes}:${secs.toString().padStart(2, '0')}`;
    };

    const handleSubmit = () => {
        if (otpKodu && otpKodu.length === 6) {
            onSubmit(otpKodu);
        }
    };

    const handleKeyPress = (e) => {
        if (e.key === 'Enter' && otpKodu && otpKodu.length === 6) {
            handleSubmit();
        }
    };

    const dialogFooter = (
        <div>
            <Button
                label="İptal"
                icon="pi pi-times"
                onClick={onHide}
                className="p-button-text"
                disabled={loading}
            />
            <Button
                label="Doğrula ve Gönder"
                icon="pi pi-check"
                onClick={handleSubmit}
                loading={loading}
                disabled={!otpKodu || otpKodu.length !== 6 || countdown === 0}
            />
        </div>
    );

    return (
        <Dialog
            header="OTP Doğrulama"
            visible={visible}
            style={{ width: '450px' }}
            footer={dialogFooter}
            onHide={onHide}
            modal
            closable={!loading}
        >
            <div className="p-fluid">
                <div className="field">
                    <label htmlFor="otpKodu">
                        E-posta adresinize gönderilen 6 haneli kodu giriniz
                    </label>
                    <InputText
                        id="otpKodu"
                        value={otpKodu}
                        onChange={(e) => setOtpKodu(e.target.value)}
                        onKeyPress={handleKeyPress}
                        maxLength={6}
                        placeholder="______"
                        autoFocus
                        disabled={loading || countdown === 0}
                        style={{ textAlign: 'center', fontSize: '1.5rem', letterSpacing: '0.5rem' }}
                    />
                    <small className="block mt-2">
                        {countdown > 0 ? (
                            <span className="text-primary">
                                <i className="pi pi-clock mr-1"></i>
                                Kalan süre: {formatTime(countdown)}
                            </span>
                        ) : (
                            <span className="text-danger">
                                <i className="pi pi-exclamation-triangle mr-1"></i>
                                OTP kodunun süresi doldu. Lütfen yeni kod talep edin.
                            </span>
                        )}
                    </small>
                </div>

                <div className="field mt-3">
                    <div className="p-message p-message-info">
                        <div className="p-message-wrapper">
                            <span className="p-message-icon pi pi-info-circle"></span>
                            <div className="p-message-text">
                                <ul className="m-0 pl-3">
                                    <li>OTP kodu 5 dakika geçerlidir</li>
                                    <li>En fazla 3 deneme hakkınız vardır</li>
                                    <li>Kod doğrulandığında bordronuz e-posta adresinize gönderilecektir</li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </Dialog>
    );
};

export default OtpDialog;
