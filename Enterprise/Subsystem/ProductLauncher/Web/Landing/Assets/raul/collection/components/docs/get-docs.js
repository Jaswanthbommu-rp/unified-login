import jsonDocs from '../../docs.json';
export default function getDocs(componentTag) {
    const componentDoc = jsonDocs.components.find(doc => doc.tag === componentTag);
    return Object.assign({}, componentDoc);
}
