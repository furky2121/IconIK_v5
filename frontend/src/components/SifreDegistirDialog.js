import React, { useState, useRef } from 'react';
import { Dialog } from 'primereact/dialog';
import { Password } from 'primereact/password';
import { Button } from 'primereact/button';
import { Toast } from 'primereact/toast';
import { Divider } from 'primereact/divider';
import authService from '../services/authService';

const SifreDegistirDialog = ({ visible, onHide }) => {
    const [loading, setLoading] = useState(false);
    const [formData, setFormData] = useState({
        mevcutSifre: '',
        yeniSifre: '',
        yeniSifreTekrar: ''
    });
    const toast = useRef(null);

    const sifreKurallari = [
        'En az 8 karakter',
        'En az 1 büyük harf (A-Z)',
        'En az 1 küçük harf (a-z)',
        'En az 1 rakam (0-9)',
        'En az 1 özel karakter (!@#$%^&*)'
    ];

    const validatePassword = (password) => {
        const errors = [];

        if (password.length < 8) {
            errors.push('Şifre en az 8 karakter olmalıdır');
        }

        if (!/[A-Z]/.test(password)) {
            errors.push('En az 1 büyük harf içermelidir');
        }

        if (!/[a-z]/.test(password)) {
            errors.push('En az 1 küçük harf içermelidir');
        }

        if (!/[0-9]/.test(password)) {
            errors.push('En az 1 rakam içermelidir');
        }

        if (!/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)) {
            errors.push('En az 1 özel karakter içermelidir');
        }

        return errors;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);

        try {
            // Validation
            if (!formData.mevcutSifre || !formData.yeniSifre || !formData.yeniSifreTekrar) {
                toast.current.show({
                    severity: 'warn',
                    summary: 'Uyarı',
                    detail: 'Tüm alanları doldurunuz',
                    life: 3000
                });
                setLoading(false);
                return;
            }

            if (formData.yeniSifre !== formData.yeniSifreTekrar) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Hata',
                    detail: 'Yeni şifreler eşleşmiyor',
                    life: 3000
                });
                setLoading(false);
                return;
            }

            // Password validation
            const passwordErrors = validatePassword(formData.yeniSifre);
            if (passwordErrors.length > 0) {
                toast.current.show({
                    severity: 'error',
                    summary: 'Şifre Kuralları',
                    detail: passwordErrors.join(', '),
                    life: 5000
                });
                setLoading(false);
                return;
            }

            const user = authService.getUser();
            const response = await authService.changePassword(
                user.id,
                formData.mevcutSifre,
                formData.yeniSifre
            );

            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Şifreniz başarıyla değiştirildi',
                    life: 3000
                });

                // Form'u sıfırla
                setFormData({
                    mevcutSifre: '',
                    yeniSifre: '',
                    yeniSifreTekrar: ''
                });

                // Dialog'u kapat
                setTimeout(() => {
                    onHide();
                }, 1500);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Şifre değiştirilemedi',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const handleInputChange = (e, field) => {
        setFormData({
            ...formData,
            [field]: e.target.value
        });
    };

    const handleDialogHide = () => {
        // Form'u sıfırla
        setFormData({
            mevcutSifre: '',
            yeniSifre: '',
            yeniSifreTekrar: ''
        });
        onHide();
    };

    const passwordHeader = <div className="font-bold mb-2">Güçlü Şifre Seçin</div>;
    const passwordFooter = (
        <div className="mt-2">
            <Divider />
            <p className="text-sm font-semibold mb-2" style={{ color: '#495057' }}>Şifre Kuralları:</p>
            <ul className="text-xs m-0 pl-3" style={{ color: '#6c757d' }}>
                {sifreKurallari.map((kural, index) => (
                    <li key={index} className="mb-1">{kural}</li>
                ))}
            </ul>
        </div>
    );

    const dialogHeader = (
        <div className="flex align-items-center gap-2">
            <i className="pi pi-lock" style={{ fontSize: '1.5rem', color: 'var(--primary-color)' }}></i>
            <span>Şifre Değiştir</span>
        </div>
    );

    return (
        <>
            <Toast ref={toast} />
            <Dialog
                header={dialogHeader}
                visible={visible}
                onHide={handleDialogHide}
                style={{ width: '450px' }}
                modal
                dismissableMask={false}
                draggable={false}
                resizable={false}
                className="p-fluid"
            >
                <form onSubmit={handleSubmit}>
                    <div className="field mb-4">
                        <label htmlFor="mevcutSifre" className="block font-medium mb-2">
                            Mevcut Şifre
                        </label>
                        <Password
                            id="mevcutSifre"
                            value={formData.mevcutSifre}
                            onChange={(e) => handleInputChange(e, 'mevcutSifre')}
                            placeholder="Mevcut şifrenizi girin"
                            className="w-full"
                            feedback={false}
                            toggleMask
                            autoComplete="current-password"
                        />
                    </div>

                    <div className="field mb-4">
                        <label htmlFor="yeniSifre" className="block font-medium mb-2">
                            Yeni Şifre
                        </label>
                        <Password
                            id="yeniSifre"
                            value={formData.yeniSifre}
                            onChange={(e) => handleInputChange(e, 'yeniSifre')}
                            placeholder="Yeni şifrenizi girin"
                            className="w-full"
                            header={passwordHeader}
                            footer={passwordFooter}
                            toggleMask
                            promptLabel="Şifre girin"
                            weakLabel="Zayıf"
                            mediumLabel="Orta"
                            strongLabel="Güçlü"
                            autoComplete="new-password"
                        />
                    </div>

                    <div className="field mb-5">
                        <label htmlFor="yeniSifreTekrar" className="block font-medium mb-2">
                            Yeni Şifre Tekrar
                        </label>
                        <Password
                            id="yeniSifreTekrar"
                            value={formData.yeniSifreTekrar}
                            onChange={(e) => handleInputChange(e, 'yeniSifreTekrar')}
                            placeholder="Yeni şifrenizi tekrar girin"
                            className="w-full"
                            feedback={false}
                            toggleMask
                            autoComplete="new-password"
                        />
                    </div>

                    <div className="flex gap-2 justify-content-end">
                        <Button
                            type="button"
                            label="İptal"
                            icon="pi pi-times"
                            onClick={handleDialogHide}
                            className="p-button-text"
                            disabled={loading}
                        />
                        <Button
                            type="submit"
                            label="Şifre Değiştir"
                            icon="pi pi-check"
                            loading={loading}
                            className="p-button-success"
                        />
                    </div>
                </form>
            </Dialog>
        </>
    );
};

export default SifreDegistirDialog;
