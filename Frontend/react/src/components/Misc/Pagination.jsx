import { useState } from 'react';

function Pagination({page, moreAvailable, onPageChange}) {
    const windowSize = 1;
    const minPage = Math.max(1, page - windowSize);
    const maxPage = moreAvailable ? page + 1 : page;
    
    const [isJumping, setIsJumping] = useState(false);
    const [jumpInput, setJumpInput] = useState('');
    
    function changePage(newPage) {
        if (newPage !== page) {
            onPageChange(newPage);
        }
    }
    
    return (
        <div className="pagination">
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
            {moreAvailable && (
                <>
                    <input type="number" min="1" max="1000" step="1"
                           placeholder="Jump to page..."
                           onChange={(event) => setJumpInput(Math.floor(event.target.value))}/>
                    <button className="calc-button" disabled={isJumping} onClick={async () => {
                        if (jumpInput !== page && jumpInput !== '') {
                            setIsJumping(true);
                            changePage(jumpInput);
                            setIsJumping(false);
                            setJumpInput('');
                        }
                    }}>Jump to page</button>
                </>
            )}
        </div>
    )
}

export default Pagination;