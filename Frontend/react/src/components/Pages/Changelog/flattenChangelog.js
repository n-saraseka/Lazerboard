export function flattenChangelog() {
    return (tree) => {
        const newChildren = [];
        let subsectionType = null;
        let listItems = [];
        let dayItems = [];
        
        for (const node of tree.children) {
            if (node.type === 'heading' && node.depth === 2) {
                if (dayItems.length > 0) {
                    if (listItems.length > 0) {
                        dayItems.push({
                            type: 'list',
                            ordered: false,
                            spread: false,
                            children: listItems,
                        });
                    }
                    newChildren.push(...dayItems);
                }
                newChildren.push(node);
                dayItems = [];
                listItems = [];
                subsectionType = null;
            }
            else if (node.type === 'heading' && node.depth === 3) {
                if (listItems.length > 0) {
                    dayItems.push({
                        type: 'list',
                        ordered: false,
                        spread: false,
                        children: listItems,
                    });
                }
                dayItems.push(node);
                listItems = [];
                subsectionType = null;
            }
            else if (node.type === 'heading' && node.depth === 4) {
                subsectionType = node.children[0]?.value?.toLowerCase();
            }
            else if (node.type === 'list') {
                let children = node.children;
                for (const child of children) {
                    child.data = {
                        ...child.data,
                        hProperties: {
                            ...child.data?.hProperties,
                            'className': `changelog-item ${subsectionType}`,
                        }
                    }
                }
                listItems.push(...children);
            }
            else if (node.type === 'heading') {
                newChildren.push(node);
            }
        }
        if (dayItems.length > 0) {
            if (listItems.length > 0) {
                dayItems.push({
                    type: 'list',
                    ordered: false,
                    spread: false,
                    children: listItems,
                });
            }
            newChildren.push(...dayItems);
        }
        
        tree.children = newChildren;
    };
}