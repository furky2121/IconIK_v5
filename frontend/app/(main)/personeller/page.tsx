import Personeller from '../../../src/pages/Personeller';

// Force dynamic rendering to prevent Calendar locale errors during build
export const dynamic = 'force-dynamic';

const PersonellerPage = () => {
    return <Personeller />;
};

export default PersonellerPage;