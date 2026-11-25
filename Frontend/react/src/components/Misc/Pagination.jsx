function Pagination({page, moreAvailable, onPageChange}) {
    const windowSize = 1;
    const minPage = Math.max(1, page - windowSize);
    const maxPage = moreAvailable ? page + 1 : page;
    
    function changePage(newPage) {
        if (newPage !== page) {
            onPageChange(newPage);
        }
    }
    
    return (
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
            {moreAvailable && <li className="page page-navigation" onClick={() => changePage(page + 1)}>&gt;</li>}
        </ul>
    )
}

export default Pagination;