import Header from '../Misc/Header.jsx';
import Footer from '../Misc/Footer.jsx';

function Page({currentPage, scoresCount, pageComponent}) {
    return <>
        <Header currentPage={currentPage}/>
        <div className="container">
            {pageComponent}
        </div>
        <Footer scoresCount={scoresCount}/>
    </>
}

export default Page;