import {useState} from "react";

function Pagination({pages, onPageChange}) {
    const windowSize = 2;
    const [page, setPage] = useState(1);
    const minPage = Math.max(1, page - windowSize);
    const maxPage = Math.min(pages, page + windowSize);
    
    function changePage(newPage) {
        if (newPage !== page) {
            setPage(newPage);
            onPageChange(newPage);
        }
    }
    
    return (
        pages > 0 &&
        <ul className="pages">
            {page !== 1 && <li className="page page-navigation" onClick={() => changePage(page - 1)}>&lt;</li>}
            {minPage !== 1 && <>
                <li className="page page-navigation" onClick={() => changePage(1)}>1</li>
                <li className="page">...</li>
            </>}
            {Array(maxPage - minPage + 1)
                .fill(0)
                .map((_, i) => 
                    <li key={i} 
                        className={`page page-navigation${minPage + i === page ? ' active' : ''}`}
                        onClick={() => changePage(minPage + i)}>
                        {minPage + i}
                    </li>)}
            {maxPage !== pages && <>
                <li className="page">...</li>
                <li className="page page-navigation" onClick={() => changePage(pages)}>{pages}</li>
            </>}
            {page !== pages && <li className="page page-navigation" onClick={() => changePage(page + 1)}>&gt;</li>}
        </ul>
    )
}

export default Pagination;